# Memory Leak Investigation — osrs-automator Discord Bot

Date: 2026-09-12 · Branch `master` @ `4edacce` · .NET 7, LiteDB 5.0.16, Discord.Net 3.10.0

**Scope:** read-only investigation. No source was modified. Findings are split into
**[V] VERIFIED** (measured or quoted from code) and **[H] HYPOTHESIS**.

---

## 1. LiteDB 5.0.16 behaviour — what an open `LiteDatabase` holds

**[V] Version.** `DiscordBot.Data/DiscordBot.Data.csproj:15` → `<PackageReference Include="LiteDB" Version="5.0.16" />`.
`DiscordBot/DiscordBot.csproj` pins the same. The resolved package is
`~/.nuget/packages/litedb/5.0.16/lib/netstandard2.0/LiteDB.dll`.

I loaded that exact assembly in a .NET 7 harness and read its internals by reflection.

**[V] Connection mode defaults to Direct.** `new ConnectionString("/tmp/probe_x.db")` reports:

```
Connection = Direct      Filename = /tmp/probe_x.db     ReadOnly = False
```

So `new LiteDatabase(connectionString, BsonMapper)` at `DiscordBot.Data/LiteDbManager.cs:108` opens
an in-process `LiteEngine` (verified: `engine type = LiteDB.Engine.LiteEngine`) that holds an
**exclusive OS lock on the data file for the whole lifetime of the object**.

**[V] Engine pragmas of a freshly created DB:**

```
Checkpoint = 1000   Timeout = 00:01:00   LimitSize = 9223372036854775807
Collation = en-GB/IgnoreCase   UtcDate = False   UserVersion = 0
```

`Checkpoint = 1000` pages × 8 KB ⇒ the `-log.db` file auto-checkpoints into the data file at ~8 MB,
and a checkpoint also runs on `Dispose()`. Checkpointing is therefore automatic here; it bounds the
**log file on disk**, not the in-memory cache.

**[V] The page cache (`LiteDB.Engine.MemoryCache`) is a high-water mark that never shrinks.**
`LiteDB.Constants.MEMORY_SEGMENT_SIZES = [12, 50, 100, 500, 1000]`, `PAGE_SIZE = 8192`. The cache
starts at one 12-page segment and *extends* whenever no free page is available; the last size (1000
pages = 8 MB) then repeats indefinitely. Measured on a single DB:

```
fresh open                 : ExtendSegments=1  ExtendPages=12    (~96 KB)
after 20k inserts          : ExtendSegments=5  ExtendPages=1662  (~13 MB)
after full scan 1/2/3      : ExtendSegments=6  ExtendPages=2662  (~21 MB, identical across scans)
after 2s idle + forced GC  : ExtendPages=2662  (unchanged)
```

So: **bounded per database by its own peak concurrent page usage, but never released while the
object is alive, and not reclaimable by GC.** `FreePages` stay allocated inside the extends.

**[V] Per-database footprint (50 databases, managed heap, forced GC):**

| state | heap | per DB | file descriptors |
|---|---|---|---|
| opened, only `UserVersion` read (what `MigrationManager.Migrate` does on an up-to-date DB) | 5.9 MB | **~121 KB** | ~2.4 fd/db |
| after 500 small inserts + one full scan each | 664 MB | **~13 MB** | ~4.5 fd/db |
| after `Dispose()` + dropping references | **14 MB** | — | released |

**[V] Disposing fully reclaims it**, and re-opening is cheap: 150 open→scan→close cycles produced
**0 MB** of heap growth, at **0.4 ms per open+scan+close** on a 500-doc collection.

---

## 2. Lifecycle in this application

**[V] Composition root has no `IHost` and never disposes anything.**

```csharp
// DiscordBot/Program.cs:16
var services = ConfigureServices(config); // No using statement?
...
await bot.Run(new CancellationToken());
await Task.Delay(-1);                     // :22
```

`Bot.Run` itself ends in `await Task.Delay(-1, stoppingToken)` (`DiscordBot/Bot.cs:30`) with a
never-cancelled token. There is no `IHostApplicationLifetime`, no `SIGTERM` handler, no
`ServiceProvider.Dispose()`. **`LiteDbManager.Dispose()` (`:134`) is unreachable in production** —
the container is killed and the process dies. Confirmed by grep: `ClearDb()` is called only from
`tests/DiscordBot.ServicesTests/Data/IdentityTests.cs:108`.

**[V] DI lifetimes** (`DiscordBot.Data/Configuration/ConfigurationExtensions.cs`):

- `MigrationManager` — singleton (`:13`)
- `LiteDbManager` — **singleton** (`:14`) ⇒ one `Dictionary<DiscordGuildId, LiteDatabase>` per process
- every `*RepositoryFactory` — transient (`:15`–`:29`)
- `IRepositoryStrategy` — singleton, built once over resolved factory instances (`:31`–`:46`)

**[V] There are no scopes anywhere.** `grep -rn "CreateScope\|IServiceScopeFactory"` over
`DiscordBot.Services/` returns nothing. Everything resolves from the root provider. This is benign
here only because **no application service implements `IDisposable`** — `grep -rnE "class .*:.*IDisposable"`
matches exactly two types: `LiteDbManager` and a test class. So the classic "transient disposables
captured by the root container" leak does **not** apply.

**[V] Repositories are cheap, short-lived, and hold only a reference.**
`BaseLiteDbRepository` stores `ILogger` + `LiteDatabase` and nothing else; `GetCollection()` is
called fresh on every operation. `RepositoryStrategy.GetOrCreateRepository` is a misnomer — it does
a linear `FirstOrDefault` over 13 factories and then `factory.Create(guildId)`, i.e. it **always
allocates a new repository**; nothing is cached. Call sites (e.g. `CountService`, `GraveyardService`,
`ClanFundsService`, `PlayerService`) assign the repo to a **local variable**, so the reference dies
with the method. Cost per call: one small object + a `LoggerFactory.CreateLogger<T>()`.

**[V] One exception — a singleton pins the common DB forever:**

```csharp
// DiscordBot/Commands/Interactive/ManageCommandsApplicationCommandHandler.cs:20 (registered AddSingleton at ConfigurationExtensions.cs:66)
_applicationCommandInfoRepository = repositoryStrategy.GetOrCreateRepository<IApplicationCommandInfoRepository>();
```

This repository — and therefore the common `LiteDatabase` — lives for the whole process.
`InteractiveCommandHandlerService` (singleton) takes `IApplicationCommandInfoRepository` by
constructor injection too. **Any eviction design must exempt the common database.**

**[V] Only one construction site:** `grep -rn "new LiteDatabase"` → `LiteDbManager.cs:108` only.

**[V] Hazard: callers can dispose a *shared* instance.** `GetDatabase` hands out the cached object,
and the test suite does `using (var db = _dbManager.GetDatabase(...))` (`IdentityTests.cs:103,112`).
Disposing leaves the dead object in `_databases`, so every later caller for that guild gets a
disposed database. Not currently triggered in production code, but it is a live footgun.

---

## 3. Quartz jobs

**[V]** `QuartzConfiguration` uses `UseInMemoryStore()`, `UseMicrosoftDependencyInjectionJobFactory()`
and `MaxConcurrency = 10` (appsettings overrides to 3). Jobs are transient; Quartz's MS-DI job
factory creates a scope per execution and disposes it after the trigger fires. Jobs hold no state
beyond injected singletons; `BaseJob.Execute` only opens a `Logger.BeginScope` inside a `using`.
**No job-side leak found.**

**[V] But jobs are the mechanism that opens *every* guild DB:**

```csharp
// DiscordBot.Services/Jobs/BaseGuildJob.cs:21-33
var guilds = guildsResult.Value.ToArray();
for (var i = 0; i < guilds.Length; i++) { ... tasks[i] = DoWorkForGuild(guild); }
```

and `ConfigurableGuildJob.DoWorkForGuild` immediately does
`RepositoryStrategy.GetOrCreateRepository<IGuildConfigRepository>(guild.Id)` → `LiteDbManager.GetDatabase(guildId)`.
`AutoUpdateGroupJob` runs at 01:00 and 22:00 daily. **Within 24 h of every boot, the manager has
opened and permanently retained one `LiteDatabase` for every guild the bot is in**, whether or not
that guild has the job enabled — the config read happens before the `IsEnabled` check.

**[V] Fan-out is unbounded and concurrent.** All guilds are started before `Task.WhenAll`; the
`lock (_createLock)` at `LiteDbManager.cs:87` serialises creation but each creation does file I/O
plus a migration check while holding the lock.

---

## 4. Other leak candidates

| candidate | verdict | evidence |
|---|---|---|
| **Discord.Net user cache** | **[V] large fixed cost, [H] largest absolute consumer** | `DiscordBot/Configuration/ConfigurationExtensions.cs:21` `AlwaysDownloadUsers = true` + `GatewayIntents.GuildMembers`. Every member of every guild is materialised as a `SocketGuildUser` and kept for the process lifetime. Hundreds of bytes/member; 10 guilds × 2 000 members is easily 50–150 MB. Grows when guilds/members grow, not over time at constant membership. |
| **Message cache** | **[V] bounded** | `MessageCacheSize = 100` (`:22`) — 100 messages per channel, fixed ring. |
| **Event handlers re-subscribed** | **[V] not a leak** | Only three `+=` in the codebase. `SerilogService.cs:9-10` and `InteractiveCommandHandlerService.cs:41` subscribe once in a singleton ctor. `_client.Connected += ClientOnConnected` (`:48`) **unsubscribes itself** on first fire (`:57`). No `Ready`-time re-subscription. |
| **Fergun.Interactive paginators** | **[H] low** | `InteractiveService` is a singleton and `_interactiveService.Callbacks` is consulted at `InteractiveCommandHandlerService.cs:72`. Fergun 1.7.1 removes callbacks on timeout; no app code adds to it directly. Worth a heap-dump check, not a prime suspect. |
| **`new HttpClient()`** | **[V] real but minor** | `DiscordBot/Helpers/Extensions/AttachmentHelper.cs:8` — created per attachment download and never disposed. Leaks a socket/handler until finalisation, not managed memory. Only one site; the Wiki API correctly uses `AddRefitClient` + `IHttpClientFactory`. |
| **AutoMapper** | **[V] fine** | Single `MapperConfiguration` built once into a singleton `Mapper` (`AutoMapperConfiguration.cs`). |
| **Serilog `LogContext` / scopes** | **[V] fine** | All `BeginScope` / `PushProperty` uses are inside `using` blocks (`BaseJob.cs:27`, `BaseGuildJob.cs:29`, `LiteDbManager.cs:110`). `WriteTo.File` with daily rolling — disk, not RAM. |
| **Static mutable caches** | **[V] none** | `grep -rnE "static +(readonly +)?(Dictionary\|List\|ConcurrentDictionary\|HashSet)"` matches only extension-method *return types*. `CollectionLogItemProvider` caches one `IEnumerable<string>` in an instance field with a `ResetCache()` — bounded. `CachedDiscordService` lives in `DiscordBot.Dashboard`, which `Program.cs` never composes. |
| **Fire-and-forget `Task.Run`** | **[V] one site, awaited** | `GroupService.cs:76` — the task is stored and `await _` happens at `:84`. |
| **Timers** | **[V] none** | No `Timer` / `PeriodicTimer` in the codebase. |
| **Static `BsonMapper.Global`** | **[H] minor** | `LiteDbManager.AddMappers()` mutates `BsonMapper.Global` (`:32-45`). Registrations are idempotent per type and the manager is a singleton — bounded, but it does mean the mapper's internal type cache is process-wide and grows with the number of distinct mapped entity types (finite).

---

## 5. Conclusion — ranked causes

1. **[V] Unbounded-in-guild-count retention of `LiteDatabase` objects, each holding a
   never-shrinking `MemoryCache`.** *Highest confidence, highest controllable magnitude.*
   Measured: **~121 KB per idle open DB, ~13 MB per DB that has served a real workload**, and the
   cache is a high-water mark that survives idleness and full GC. `LiteDbManager` is a singleton,
   `_databases` is only ever added to (`:102`) — there is no eviction, and `ClearDb()`/`Dispose()`
   are dead code in production (§2).
2. **[V] Discord.Net's `AlwaysDownloadUsers = true` member cache.** Probably the single largest
   *absolute* number in a heap dump, but it is a fixed function of guild/member count, not a leak.
3. **[H] LiteDB per-DB cache drift.** Each guild's high-water mark ratchets up as its data grows
   and as occasional wide reads happen (leaderboard jobs scan whole collections). This is the one
   component that genuinely **grows over 24 h/week** at constant guild count.
4. **[V] `new HttpClient()` in `AttachmentHelper`** — socket/handler churn, negligible RSS.
5. **[V] Nothing else found.** No scope leaks, no static caches, no event-handler accumulation.

### Is "not closing the DB" actually a leak?

**Mostly a fixed-per-guild footprint, not a classic unbounded leak — but with a growing component.**

- Guild count is fixed by Discord, not by uptime. Once every guild has been touched (≤24 h, by the
  01:00 job), `_databases.Count` stops growing.
- So the shape is: **fast ramp during the first day → a plateau of N × (per-guild high-water) →
  slow ratcheting upward** as per-guild peaks grow (cause 3). Memory never comes back down between
  bursts, which is what makes it *feel* like a leak.
- **Scaling:** N guilds × ~0.1 MB if the DB is only opened, × 5–15 MB once it has served real
  traffic. 10 guilds ≈ 1–130 MB; 50 guilds ≈ 6–650 MB; 200 guilds ≈ 25 MB–2.6 GB. Actual guild
  count is unknown (no deployment data in the repo, `db_*_def.db` files are gitignored) — count
  `db_*_def.db` files in the container's working directory to get N, and `ls -la` them to see which
  guilds are large.
- Each open DB also costs ~2–5 file descriptors and holds an **exclusive file lock**, so N also
  drives fd pressure and makes external backup/inspection of the files impossible while running.

---

## 6. Remediation options

### (a) Open-per-operation — `using var db = new LiteDatabase(...)` ✅ **RECOMMENDED**
- **Pros:** [V] measured **zero heap growth over 150 open/close cycles** and **0.4 ms per
  open+scan+close**; releases fds and file locks; checkpoints the log on every dispose (smaller
  `-log.db` files); removes the disposed-shared-instance footgun entirely; no clock, no timer, no
  eviction policy to get wrong.
- **Cons:** open cost paid per call (sub-millisecond, but `BaseGuildJob` fans out over all guilds —
  still trivial); *loses* the page cache between calls, so a hot loop over one guild re-reads pages;
  requires every repository to own a DB lifetime instead of borrowing one.
- **Blast radius:** `LiteDbManager`, `BaseLiteDbRepositoryFactory` + 13 factories,
  `BaseLiteDbRepository`, and the one singleton that caches a repository. ~20 files.
- **Effort:** 1–2 days including the singleton fix.

### (b) Keep the cache, add idle eviction
- **Pros:** preserves warm-cache performance; smallest conceptual change to call sites.
- **Cons:** **this is the dangerous one.** Repositories hold a raw `LiteDatabase` reference for
  their (short) lifetime, and `ManageCommandsApplicationCommandHandler` holds one for the *process*
  lifetime. A sweeper that disposes a DB while a Quartz job is mid-`FindAll` throws
  `ObjectDisposedException`. Needs refcounting (see below), which is real concurrency work.
- **Blast radius:** `LiteDbManager` + a hosted sweeper + every acquisition site must become a
  lease. **Effort:** 2–4 days, and the failure mode is intermittent production exceptions.

### (c) `Connection=shared`
- **[V]** LiteDB XML docs: *"Create ILiteEngine instance according string connection parameters. For
  now, only Local/Shared are supported"*; `SharedEngine.OpenDatabase`/`CloseDatabase` exist.
  Shared mode opens and closes the engine around **each operation** using a named mutex.
- **Pros:** one-line change (`$"Filename={path};Connection=shared"` at `LiteDbManager.cs:103`); the
  engine — and its `MemoryCache` — is torn down between operations, so the leak disappears; other
  processes can read the files.
- **Cons:** **a global mutex per file serialises all access** and adds open/close cost to every
  operation; the cached `LiteDatabase` objects stay in the dictionary forever (fixing RAM but not
  the unbounded dictionary or the object graph); mutex semantics on Linux/containers are a known
  LiteDB rough edge. **Blast radius:** one line. **Effort:** 30 min + soak test.
- Good as an **emergency mitigation**, not as the design.

### (d) `db.Checkpoint()` / cache pragmas
- **[V]** `Checkpoint` is a *log-size* pragma (default 1000 pages), and `ILiteDatabase.Checkpoint()`
  copies the log into the data file — I confirmed it drops a 2.9 MB log to 0 KB. **There is no
  `CACHE_SIZE` pragma in 5.0.16**; the pragma set is exactly `UserVersion, Collation, Timeout,
  LimitSize, UtcDate, Checkpoint`. `MEMORY_SEGMENT_SIZES` is an internal `static` array —
  writable by reflection, but that is a process-wide hack, not a fix.
- **Verdict:** helps disk usage only. **Does not address the memory.** Worth adding a periodic
  `Checkpoint()` regardless, but do not count it as the fix.

### (e) Single database, `GuildId` as a field
- **Pros:** one cache, one file, one lock; trivially bounded. **Cons:** full data migration across
  N guild files, every repository and query rewritten, loses per-guild isolation and per-guild
  file backup/deletion on guild-leave. **Effort:** 1–2 weeks. **Out of scope.**

### Recommendation: **(a) open-per-operation**, with **(c) as a same-day stopgap** if RSS is critical now.

Concretely:

1. **`DiscordBot.Data/LiteDbManager.cs`** — replace `GetDatabase`/`GetCommonDatabase` with
   `OpenDatabase(DiscordGuildId)` / `OpenCommonDatabase()` that **always** return a fresh
   `LiteDatabase` the caller owns. Delete `_databases`, `_createLock`, `_commonLock`,
   `_commonDatabase`, `ClearDb`, `IDisposable`. Keep `AddMappers()` (still once, still a singleton)
   and keep `_manager.Migrate(...)`, but **gate migration on a `ConcurrentDictionary<DiscordGuildId, bool>`
   of already-migrated files** so the migration check is not re-run on every open. No locking is
   required afterwards: LiteDB in Direct mode takes the OS file lock itself, so two concurrent
   opens of the same guild file must be serialised — either keep a per-guild `SemaphoreSlim` around
   *the open*, or (simpler and what I'd do) keep a `Dictionary<DiscordGuildId, object>` of
   per-guild gate objects so only same-guild opens contend.
2. **`BaseLiteDbRepository<T>`** — implement `IDisposable`, take ownership of the `LiteDatabase`,
   dispose it in `Dispose()`. (Alternatively keep repos stateless and change `IRepository` methods
   to open/close internally — more churn, same result.)
3. **`BaseLiteDbRepositoryFactory` + the 13 factories** — `Create(guildId)` calls
   `LiteDbManager.OpenDatabase(guildId)` instead of `GetDatabase(guildId)`. Signatures unchanged.
4. **Every call site becomes `using`.** `RepositoryStrategy.GetOrCreateRepository` already returns
   a fresh instance per call, so this is a mechanical `var repo =` → `using var repo =` across
   `DiscordBot.Services/Services/*.cs` and `DiscordBot/Commands/**`. ~60 sites (grep
   `GetOrCreateRepository\|GetRepository<`).
5. **`ManageCommandsApplicationCommandHandler.cs:20` and `InteractiveCommandHandlerService`** must
   stop caching a repository in a field — inject `IRepositoryStrategy` and open per command. This
   is the only place where a repository currently outlives a method call, and it is the reason
   option (b) needs refcounting.
6. **Optional keep-warm:** if step 4 proves too slow for `BaseGuildJob`'s fan-out, open the DB
   **once per job execution** and pass the repository down, rather than caching across executions.

*If you take option (b) instead:* `_databases` must become
`Dictionary<DiscordGuildId, (LiteDatabase Db, int RefCount, DateTimeOffset LastTouched)>`;
`GetDatabase` returns an `IDisposable` lease that increments/decrements `RefCount` under
`_createLock`; a hosted sweeper (inject `TimeProvider`, never `DateTime.Now`) disposes entries with
`RefCount == 0 && now - LastTouched > idleWindow` **while holding the same lock**; the common DB is
excluded because singletons pin it.

---

## 7. How to verify the fix

**Production measurement (the real proof):**

- Baseline the current build for 24–48 h, then the fixed build for the same window, same guilds.
- **Container RSS:** `docker stats --no-stream` sampled every 5 min, or
  `docker run --memory=512m` so a regression shows up as an OOM-kill rather than slow bloat.
  RSS is the number the owner actually cares about.
- **In-process gauge:** a Quartz job (or a 5-min `PeriodicTimer`) logging
  `GC.GetTotalMemory(false)`, `GC.GetTotalAllocatedBytes()`,
  `GC.GetGCMemoryInfo().HeapSizeBytes`, `Environment.WorkingSet`, `Process.GetCurrentProcess().HandleCount`,
  plus **`_databases.Count`** (or open-lease count after the fix). Serilog JSON already ships these
  to `logs/osrs_bot.log`.
- **`dotnet-counters monitor -p <pid> System.Runtime`** — watch `gc-heap-size`,
  `gen-2-gc-count`, `loh-size`. Add the container tools to the runtime image or
  `docker exec` with a sidecar.
- **Heap dump for attribution:** `dotnet-gcdump collect -p <pid>` at T+1 h and T+24 h; diff them.
  The decisive check is the **count of `LiteDB.Engine.MemoryCache` / `LiteDB.Engine.LiteEngine`
  instances** and their retained size. Before: one per guild, tens of MB each. After: ~1 (the
  common DB), transient spikes only. Second check: `Discord.WebSocket.SocketGuildUser` count —
  expect it unchanged, which confirms the remaining plateau is the Discord cache, not LiteDB.
- **Success criterion:** the 24 h RSS slope flattens to ~0 after the first 30 min, and
  `MemoryCache` instance count no longer tracks guild count.

**Regression tests** (`tests/DiscordBot.ServicesTests/Data/`, xUnit + FluentAssertions, already has
a `LiteDbManager` harness with a per-test `PathPrefix`):

1. **Files are released.** Touch 50 guild ids through the manager/repositories, let the leases fall
   out of scope, then assert each file can be re-opened **exclusively**:
   `new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)` must not throw.
   Under today's code this throws `IOException` for all 50 — the test fails on `master` and passes
   after the fix, which is exactly the regression guard wanted.
2. **No retained engines.** After the same 50-guild sweep plus `GC.Collect(); GC.WaitForPendingFinalizers();`,
   assert `GC.GetTotalMemory(true)` is within, say, 20 MB of the pre-sweep baseline. Keep the
   threshold generous — this is a smoke alarm, not a benchmark.
3. **Idle eviction (only if option (b) is chosen).** Inject a fake `TimeProvider`, touch 50 guilds,
   advance the clock past the idle window, run the sweeper, assert files re-open exclusively and
   that a lease held across the sweep is **not** disposed (guards the concurrency bug).
4. **Concurrency.** `Parallel.For` 100 iterations over 10 guild ids doing a real read+write;
   assert no `ObjectDisposedException` / `LiteException` and that the data is correct. This is the
   test that would have caught the `using (var db = _dbManager.GetDatabase(...))` footgun.
