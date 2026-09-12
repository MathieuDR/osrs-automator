# LiteDB lifecycle: close guild databases when not in use

Date: 2026-09-12 · Status: **implemented on feature/hosting-tracker-and-litedb-lifecycle** · Scope: `DiscordBot.Data`, call sites in `DiscordBot` and `DiscordBot.Services`, tests.

Background: `docs/codebase/03-data-layer.md`, `docs/codebase/06-memory-leak-investigation.md`.

## 1. Problem

`LiteDbManager` (`DiscordBot.Data/LiteDbManager.cs`) is a singleton that opens one `LiteDatabase` per Discord guild on first use and keeps it in `_databases` forever. `Dispose()`/`ClearDb()` are unreachable in production (`Program.cs` never disposes the provider; the container is simply killed).

Measured (LiteDB 5.0.16, see doc 06):

| state | per open database |
|---|---|
| opened, idle | ~121 KB + 2–5 file descriptors + exclusive file lock |
| after real reads/writes | ~5–15 MB (page cache is a high-water mark, never shrinks) |
| after `Dispose()` | fully reclaimed |

`AutoUpdateGroupJob` fans out over **every** guild at 01:00 and 22:00 and reads `GuildConfig` before checking whether the job is enabled, so within 24 h of boot every guild's database is open and retained. Result: RSS ramps for a day, plateaus at N guilds × up to 15 MB, and ratchets upward as per-guild peaks grow. Not a classic unbounded leak, but it is the largest **controllable** part of the footprint. (The largest fixed cost is Discord.Net's `AlwaysDownloadUsers = true` member cache; out of scope, see §8.)

## 2. Goal and non-goals

**Goal:** a guild database is open only while something is actually using it, and its memory is released when the last user finishes. Memory should plateau within minutes of boot at roughly "common DB + whatever is in flight", independent of guild count.

**Non-goals:** consolidating into one database, changing LiteDB version or connection mode, fixing `AlwaysDownloadUsers`, adding a proper `IHost`. Each is listed as a follow-up in §8.

## 3. Design: reference-counted leases

Rejected alternatives (details in doc 06 §6):

- **Plain open-per-operation** (`using var db = new LiteDatabase(...)` in every repository). Direct mode takes an exclusive file lock for the object's lifetime, so two concurrent operations on the same guild (a command and a job, or two users) would collide. Serialising with a per-guild semaphore fixes that but creates a self-deadlock hazard: `ConfigurableGuildJob` holds a `GuildConfig` repository while the concrete job opens a second repository for the same guild.
- **Idle-timer eviction of the current cache.** Needs the same refcounting as below plus a sweeper and a fake clock; strictly more machinery for the same result.
- **`Connection=shared`.** One-line change and a valid emergency stopgap, but it wraps every operation in a cross-process named mutex and keeps the dictionary of `LiteDatabase` objects growing.

Chosen: **the manager hands out leases; the database is opened on the first lease for a guild and disposed when the last lease is released.** Concurrent users of the same guild share one instance (no lock collisions), nested acquisition just bumps the count (no deadlocks), and idle guilds hold zero memory.

### 3.1 `LiteDbManager` (rewrite of the public surface, same file)

```csharp
public sealed class DatabaseLease : IDisposable {
    public LiteDatabase Database { get; }
    // Release() is idempotent; second Dispose is a no-op.
}

public class LiteDbManager {
    public DatabaseLease Lease(DiscordGuildId guildId);   // per-guild file
    public DatabaseLease LeaseCommon();                    // common file
    public int OpenDatabaseCount { get; }                  // for the metrics log line, §6
}
```

Internals:

- `Dictionary<string /*file path*/, Entry>` where `Entry = { LiteDatabase Db; int RefCount; }`, guarded by one `object _gate`. The common database is just another key, so it gets the same treatment.
- `Lease(...)`: under the gate, get-or-create the entry (create = `CreateDirectory` + `new LiteDatabase(path, BsonMapper)` + `_manager.Migrate(db)`, exactly as today), increment `RefCount`, return a lease bound to that key.
- `Release(key)`: under the gate, decrement; if zero, remove the entry and dispose the `LiteDatabase` **after** leaving the lock (dispose runs a checkpoint; a few ms of I/O should not block other guilds).
- Keep `AddMappers()` and the `BsonMapper` property unchanged; the mapper is global and registered once.
- Remove `GetDatabase`, `GetCommonDatabase`, `ClearDb`, `IDisposable`, `_databases`, `_commonDatabase`, the two locks. `KillBotCommandHandler` currently calls `_manager.Dispose()` before `Environment.Exit(0)`; replace with nothing (open leases at that instant are in-flight operations; LiteDB's log file makes the file crash-safe) or, if preferred, a `DisposeAll()` that force-disposes remaining entries for a clean checkpoint. Recommendation: add `DisposeAll()` and call it from `/kill`, it is ten lines and keeps that command's intent.
- Migration runs on every open (it only reads `UserVersion` on an up-to-date file, sub-millisecond). Optional: memoise "already migrated" per path in a `HashSet<string>` under the same gate.

- **Rollback switch:** add `bool CloseWhenUnused { get; set; } = true;` to `LiteDbOptions` (bound from `LiteDbOptions` in `appsettings`). When `false`, `Release` decrements the count but never disposes, which is exactly today's behaviour. If anything misbehaves in production, flip the setting and restart; no redeploy, no code change.

Measured open+scan+close is ~0.4 ms, so re-opening per interaction is not a performance concern. If job fan-out ever shows up in logs, an **optional linger** (keep the entry alive for e.g. 30 s after the count hits zero, via a `TimeProvider`-driven timer) can be added later without changing the public API. Do not build it now.

### 3.2 Repositories own a lease

- `IRepository` (marker interface, `DiscordBot.Data/Interfaces/IRepository.cs`) becomes `IRepository : IDisposable`. Every repository interface already derives from it, so all of them become disposable in one edit.
- **Materialise every query result.** `BaseLiteDbRepository.GetAll()` and `BaseRecordLiteDbRepository.GetAll()` currently return `GetCollection().FindAll()`, which is a **lazy** enumerable over the LiteDB engine. Today that is harmless because the database never closes; after this change a caller that enumerates after disposing the repository would get `ObjectDisposedException`. Fix at the source: `FindAll().ToList()` in both base classes, and audit every concrete repository for `Find(...)`/`Query()` chains that are not terminated by `ToList/ToArray/First*/Single*/Count` (grep in `DiscordBot.Data/Repository/`; as of today only the two `GetAll` sites and one multi-line `Query()` in `ConfirmationRepository.cs:15` need checking). Collections are small (per-guild config, counts, players), so materialising costs nothing measurable.
- `BaseLiteDbRepository<T>` and `BaseRecordLiteDbRepository<T>` take a `DatabaseLease` instead of a `LiteDatabase`. `LiteDatabase` property becomes `_lease.Database`. Implement `Dispose()` as `_lease.Dispose()`. `BaseSingleRecordLiteDbRepository<T>` inherits it.
- The 13 factories: `LiteDbManager.GetDatabase(guildId)` → `LiteDbManager.Lease(guildId)`; `GetCommonDatabase()` → `LeaseCommon()`. Constructor signatures of concrete repositories change from `LiteDatabase` to `DatabaseLease`. Nothing else in the factories changes.
- `RepositoryStrategy` is unchanged; it already returns a fresh repository per call.

### 3.3 Call sites: `using var repo = …`

26 call sites of `GetOrCreateRepository` outside `DiscordBot.Data` (grep `GetOrCreateRepository` in `DiscordBot/`, `DiscordBot.Services/`). Each becomes `using var repo = RepositoryStrategy.GetOrCreateRepository<…>(guildId);`. The repository must not escape the method. Where a repository is returned from a helper (e.g. `BaseGuildConfigurationService`, `RepositoryService` in `DiscordBot.Services/Services/`), the caller owns disposal; keep helper shapes but document ownership in a doc comment.

Repositories that are **held longer than a method** today, and what to do:

| where | today | change |
|---|---|---|
| `DiscordBot/Commands/Interactive/ManageCommandsApplicationCommandHandler.cs:20` | singleton caches `IApplicationCommandInfoRepository` in a field | inject `IRepositoryStrategy`; `using var repo = …GetOrCreateRepository<IApplicationCommandInfoRepository>()` inside each method that needs it |
| `DiscordBot/Services/InteractiveCommandHandlerService.cs:25` | singleton ctor-injects the repository | same: inject `IRepositoryStrategy`, lease per use (`InitializeCommands`) |
| `DiscordBot/Services/CommandRegistrationService.cs:14`, `CommandDefinitionRegistrationService.cs:14`, and the factory lambda at `DiscordBot/Configuration/ConfigurationExtensions.cs:135` | ctor-inject the repository | same |
| `DiscordBot.Data/Configuration/ConfigurationExtensions.cs:21-22` | `AddTransient(x => factory.Create())` registrations for `IApplicationCommandInfoRepository` and `IRuneScapeDropDataRepository` | **delete both**. Once repositories are `IDisposable`, resolving them from the root provider makes the container track them forever (a genuine leak). Fix the consumers that break, using the strategy instead. |

### 3.4 Tests

`tests/DiscordBot.ServicesTests/Data/IdentityTests.cs` uses `_dbManager.GetDatabase(...)` directly, sometimes inside `using`. Update to `using var lease = _dbManager.Lease(...); var db = lease.Database;`.

## 4. Behavioural guarantees to verify

1. **Files are released.** Lease 50 guild ids, dispose the leases, then `new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)` succeeds for each file. (Fails on `master` today.)
2. **Shared instance while overlapping.** Two live leases for the same guild expose the same `LiteDatabase`; disposing one keeps the other usable; disposing the second releases the file.
3. **Nested acquisition does not deadlock.** Acquire a `IGuildConfigRepository` and, while holding it, an `IPlayerRepository` for the same guild; both work.
4. **Concurrency.** `Parallel.For` 200 iterations over 10 guild ids, each doing lease + insert + read + dispose; no `ObjectDisposedException`/`LiteException`, `OpenDatabaseCount == 0` at the end.
5. **Double dispose is safe.** Disposing a lease twice does not underflow the count.
6. **Memory smoke test.** After the 50-guild sweep + `GC.Collect()`, `GC.GetTotalMemory(true)` within 20 MB of baseline. Generous threshold, it is an alarm not a benchmark.

Tests live next to `IdentityTests.cs`, reuse its per-test `PathPrefix` harness, xUnit + FluentAssertions.

## 5. Implementation order

1. `LiteDbManager` + `DatabaseLease` + new tests (compile the Data project alone; nothing else uses the new API yet).
2. `IRepository : IDisposable`, base repositories, 13 factories. Solution will fail to compile at every call site, which is the checklist.
3. Fix the four singleton/DI pins in §3.3 first (they are the ones that matter), then the mechanical `using var` sweep.
4. Update `IdentityTests`, run `dotnet test tests/DiscordBot.ServicesTests` (baseline on `master`: 57 pass / 5 fail / 3 skip, failures pre-existing; do not regress).
5. `dotnet build OrsrDiscordAutomator.sln` must also compile `DiscordBot.Dashboard` (it composes the bot as a hosted service).
6. Metrics log line (§6), then deploy and observe.

Estimated size: ~25 files, one focused PR.

## 6. Verification in production

Add one line of observability so the fix is measurable: a Quartz job `MemoryReportJob : BaseJob` every 30 min logging `Environment.WorkingSet`, `GC.GetTotalMemory(false)`, `Process.GetCurrentProcess().HandleCount`, and `LiteDbManager.OpenDatabaseCount`. Serilog already writes JSON to `logs/osrs_bot.log`. Register it in `ServiceConfigurationExtensions.ConfigureJobs`.

Success criterion: after the first 30 min, `OpenDatabaseCount` sits at 0–2 between interactions, and 24 h RSS in `docker stats` is flat instead of ramping. Optional deep check: `dotnet-gcdump` at T+1 h and T+24 h; the count of `LiteDB.Engine.LiteEngine` instances should no longer track guild count.

## 7. Risks and confidence

Honest assessment of "will this break the bot":

**What makes the change well-bounded (verified in code):**
- Exactly one place constructs `LiteDatabase` (`LiteDbManager.cs:108`); no other type in the solution touches a `LiteDatabase` except repositories and `IdentityTests`.
- Repositories hold only a logger and the database reference; the strategy already creates a fresh one per call. Making them disposable changes ownership, not behaviour.
- No DI scopes anywhere and no other `IDisposable` services, so the disposable-in-root-container problem is limited to the two `AddTransient(… .Create())` registrations this spec deletes.
- The lease design shares one instance between concurrent users and tolerates nested acquisition, so the two failure modes of naive open-per-operation (file-lock collisions, self-deadlock in `ConfigurableGuildJob`) cannot occur.
- Data files, format, and migrations are untouched. Dispose runs LiteDB's normal checkpoint, the same thing `/kill` does today. Taking a copy of `db_*.db` before the first deploy is still cheap insurance.
- The compiler enforces the call-site sweep: every `GetOrCreateRepository` result must now be disposed or the analyser/`using` review catches it.

**What can still go wrong, and the mitigation for each:**

| risk | consequence | mitigation |
|---|---|---|
| lazy `FindAll()` enumerated after dispose | `ObjectDisposedException` at a call site | materialise in the base repositories (§3.2); this is the one genuine behavioural trap and it is fixed centrally |
| a `using` is forgotten | that guild's DB stays open until exit = today's behaviour, never worse | optional `DEBUG` finaliser on `DatabaseLease` logging the path |
| a repository stashed in a field of a singleton | that DB pinned forever; also harmless | four known sites (§3.3); grep field declarations ending in `Repository;` after the change |
| thin automated coverage: 57 pass / 5 pre-existing failures / 3 skipped, and **no tests for services or jobs** | a regression in a job or command path would surface in production, not CI | (1) the six new tests in §4 cover the data layer directly; (2) run every slash command once in a dev guild and let the 01:00/22:00 jobs fire once before calling it done; (3) `CloseWhenUnused=false` reverts to today's behaviour with a config flip |
| dispose cost on every idle transition | sub-millisecond per close for small files | add the linger (§3.1) only if the metrics line shows it matters |

Net: high confidence in the data-layer change itself, moderate confidence that all 26 call sites are behaviourally identical on day one because the existing test suite would not tell us. The rollback flag and the metrics job turn the residual risk into a config flip rather than a hotfix.
## 8. Follow-ups (not in this spec)

- `AlwaysDownloadUsers = true` (`DiscordBot/Configuration/ConfigurationExtensions.cs:21`) is likely the largest fixed cost. Turning it off breaks `Guild.GetUser(id)` lookups used for display names; would need `DownloadUsersAsync` on demand. Separate decision.
- `new HttpClient()` per attachment in `DiscordBot/Helpers/Extensions/AttachmentHelper.cs:8`: use `IHttpClientFactory`. Minor.
- Give the console host a real `IHost`/`SIGTERM` path so `docker stop` checkpoints cleanly.
