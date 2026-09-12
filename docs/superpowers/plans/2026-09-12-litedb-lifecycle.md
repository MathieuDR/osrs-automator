# LiteDB Lifecycle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Guild LiteDB files are open only while a repository is using them, so memory no longer scales with guild count.

**Architecture:** `LiteDbManager` hands out reference-counted `DatabaseLease`s; the database opens on the first lease for a file and is disposed when the last lease is released. Repositories own one lease each and become `IDisposable`; every call site uses `using var repo = …`. A `CloseWhenUnused=false` option restores today's keep-open behaviour as a rollback switch.

**Tech Stack:** .NET 7, LiteDB 5.0.16, xUnit + FluentAssertions + NSubstitute (existing test project `tests/DiscordBot.ServicesTests`).

**Spec:** `docs/superpowers/specs/2026-09-12-litedb-lifecycle-design.md`. Read it first, plus `docs/codebase/03-data-layer.md` and `06-memory-leak-investigation.md`.

## Global Constraints

- Do not change LiteDB version, connection mode, file naming (`{PathPrefix}{guildId|common}_{FileSuffix}.db`), collection names, or models. No data migration.
- `BsonMapper` registration in `LiteDbManager.AddMappers()` stays exactly as is (it mutates the global mapper once).
- Materialise every query result inside repositories (`ToList()`); nothing lazy may cross a repository boundary.
- Build with `dotnet build OrsrDiscordAutomator.sln`; test with `dotnet test tests/DiscordBot.ServicesTests/DiscordBot.ServicesTests.csproj`. Baseline on `master`: 57 passed / 5 failed / 3 skipped; the 5 failures are pre-existing and must not grow.
- Each task ends with a commit on branch `feature/hosting-tracker-and-litedb-lifecycle`. Commit messages end with `Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>`.
- TDD cycle for every task: write the test, run it and see it fail for the right reason, implement, run again, commit. The steps below name the tests and behaviour; write the boilerplate yourself following `tests/DiscordBot.ServicesTests/Data/IdentityTests.cs` conventions (Arrange/Act/Assert comments, FluentAssertions).

---

### Task 1: `DatabaseLease` and the refcounted `LiteDbManager`

**Files:**
- Create: `DiscordBot.Data/DatabaseLease.cs`
- Modify: `DiscordBot.Data/LiteDbManager.cs` (rewrite everything except `AddMappers`/`AddDictMapper`/`BsonMapper`)
- Modify: `DiscordBot.Data/Configuration/LiteDbOptions.cs` (add `public bool CloseWhenUnused { get; set; } = true;`)
- Modify: `tests/DiscordBot.ServicesTests/Data/IdentityTests.cs` (adapt to the new API; keep every existing assertion)
- Create: `tests/DiscordBot.ServicesTests/Data/LiteDbManagerLeaseTests.cs`

**Interfaces (produces):**

```csharp
namespace DiscordBot.Data;

public sealed class DatabaseLease : IDisposable {
    public LiteDatabase Database { get; }     // throws ObjectDisposedException after Dispose
    public string FilePath { get; }
    public void Dispose();                    // idempotent
}

public class LiteDbManager {
    public LiteDbManager(ILogger<LiteDbManager> logger, IOptions<LiteDbOptions> options, MigrationManager manager);
    public BsonMapper BsonMapper { get; set; }
    public DatabaseLease Lease(DiscordGuildId guildId);
    public DatabaseLease LeaseCommon();
    public int OpenDatabaseCount { get; }
    public void DisposeAll();                 // force-dispose every open database (used by /kill and tests)
    public static void AddDictMapper<TIdentity, TObject>(Func<ulong, TIdentity> ctor) where TIdentity : new(); // unchanged
}
```

**Implementation sketch (the non-obvious part):**

```csharp
private sealed class Entry { public LiteDatabase Db; public int RefCount; }
private readonly object _gate = new();
private readonly Dictionary<string, Entry> _open = new();

public DatabaseLease Lease(DiscordGuildId guildId) => LeaseFile(GetGuildFileName(guildId));
public DatabaseLease LeaseCommon() => LeaseFile(CreateConnectionString());

private DatabaseLease LeaseFile(string path) {
    lock (_gate) {
        if (!_open.TryGetValue(path, out var entry)) {
            entry = new Entry { Db = CreateDatabase(path) };   // existing CreateDirectory + new LiteDatabase + Migrate
            _open[path] = entry;
        }
        entry.RefCount++;
        return new DatabaseLease(this, path, entry.Db);
    }
}

internal void Release(string path) {
    LiteDatabase toDispose = null;
    lock (_gate) {
        if (!_open.TryGetValue(path, out var entry)) return;
        entry.RefCount--;
        if (entry.RefCount <= 0 && _options.CloseWhenUnused) {
            _open.Remove(path);
            toDispose = entry.Db;
        }
    }
    toDispose?.Dispose();   // outside the lock: dispose runs a checkpoint
}
```

`DatabaseLease.Dispose()` uses `Interlocked.Exchange` on a flag so a second call is a no-op, then calls `manager.Release(path)`.

- [ ] **Step 1: Write failing tests in `LiteDbManagerLeaseTests.cs`** (same fixture setup as `IdentityTests`: `Substitute.For<MigrationManager>(NullLoggerFactory.Instance)`, `NullLogger`, unique `PathPrefix`, delete files in `Dispose`). Helper: `bool FileIsUnlocked(string path)` = `try { using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None); return true; } catch (IOException) { return false; }`. Path for guild g = `$"{prefix}{g}_{suffix}.db"`.
  1. `Lease_ThenDispose_ReleasesFile`: lease guild 10, insert one `TestModel`, dispose lease → `FileIsUnlocked` true, `OpenDatabaseCount == 0`.
  2. `Lease_WhileHeld_LocksFile`: lease held → `FileIsUnlocked` false, `OpenDatabaseCount == 1`.
  3. `TwoLeases_SameGuild_ShareInstance`: `a.Database` is `Same` as `b.Database`; dispose `a` → `b.Database.GetCollection<TestModel>("m").Count()` still works, count still 1; dispose `b` → file unlocked.
  4. `Leases_DifferentGuilds_OpenSeparateDatabases`: guilds 1 and 2 → `OpenDatabaseCount == 2`, instances differ.
  5. `Dispose_Twice_DoesNotUnderflow`: dispose the same lease twice, then lease again and use it; `OpenDatabaseCount == 1` while held, 0 after.
  6. `CommonLease_BehavesLikeGuildLease`: `LeaseCommon()` file `{prefix}common_{suffix}.db` locked while held, unlocked after.
  7. `CloseWhenUnused_False_KeepsDatabaseOpen`: manager built with `CloseWhenUnused = false`; lease + dispose → file still locked, `OpenDatabaseCount == 1`; `DisposeAll()` → unlocked, count 0.
  8. `Parallel_Access_SameGuilds_IsSafe`: `Parallel.For(0, 200, i => { using var l = mgr.Lease(new DiscordGuildId((ulong)(i % 10))); var c = l.Database.GetCollection<TestModel>("m"); c.Insert(new TestModel{Name=i.ToString(), UserId=new DiscordUserId(1)}); c.Count().Should().BeGreaterThan(0); })` → no exception, afterwards `OpenDatabaseCount == 0` and each of the 10 files unlocked.
  9. `DataPersists_AcrossLeases`: insert under lease A, dispose, lease B, read → the document is there (proves dispose checkpointed).
- [ ] **Step 2: Run** `dotnet test tests/DiscordBot.ServicesTests/DiscordBot.ServicesTests.csproj --filter LiteDbManagerLeaseTests` → compile errors (no `Lease`).
- [ ] **Step 3: Implement** `DatabaseLease`, rewrite `LiteDbManager` as sketched, add `CloseWhenUnused` to `LiteDbOptions`. Delete `GetDatabase`, `GetCommonDatabase`, `ClearDb`, `IDisposable`, `_databases`, `_commonDatabase`, `_commonLock`, `_createLock`. Keep `CreateConnectionString`, `GetGuildFileName`, `CreateDatabase`, `CreateDirectory`, and the `LogContext.PushProperty("db", …)` around `Migrate`.
- [ ] **Step 4: Adapt `IdentityTests.cs`**: `_dbManager.Dispose()` → `_dbManager.DisposeAll()`; every `_dbManager.GetDatabase(x)` → `_dbManager.Lease(x)` with `.Database`; the `using (var db = …)` blocks become `using (var lease = …) { var db = lease.Database; … }`; `_dbManager.ClearDb()` → `_dbManager.DisposeAll()`. Assertions unchanged. (This project will not compile until Task 2 too, because factories still call `GetDatabase`; that is expected. Run only after Task 2, or temporarily stub. Preferred: do Tasks 1 and 2 before the first test run, committing separately.)
- [ ] **Step 5: Commit** `feat(data): reference-counted LiteDB leases in LiteDbManager`.

### Task 2: Repositories own a lease; factories use `Lease`/`LeaseCommon`

**Files:**
- Modify: `DiscordBot.Data/Interfaces/IRepository.cs` → `public interface IRepository : IDisposable { }`
- Modify: `DiscordBot.Data/Repository/BaseLiteDbRepository.cs`, `BaseRecordLiteDbRepository.cs` (ctor takes `DatabaseLease lease`; `LiteDatabase => _lease.Database`; `public void Dispose() => _lease.Dispose();`; `GetAll()` returns `GetCollection().FindAll().ToList()`)
- Modify: `DiscordBot.Data/Repository/BaseSingleRecordLiteDbRepository.cs` (ctor param type)
- Modify: every concrete repository ctor in `DiscordBot.Data/Repository/*.cs` (13 files: `LiteDatabase database` → `DatabaseLease lease`, pass through)
- Modify: all 13 factories in `DiscordBot.Data/Factories/*.cs`: `LiteDbManager.GetDatabase(guildId)` → `LiteDbManager.Lease(guildId)`, `GetCommonDatabase()` → `LeaseCommon()`
- Audit: `DiscordBot.Data/Repository/ConfirmationRepository.cs:15` already terminates with `FirstOrDefault()`; check every other `Find`/`Query` in the folder terminates (`ToList/ToArray/First*/Single*/Count`); fix any that do not.

**Interfaces (consumes):** `DatabaseLease`, `LiteDbManager.Lease/LeaseCommon` from Task 1. **Produces:** every `I*Repository` is `IDisposable`; `factory.Create(...)` returns a repository that owns exactly one lease.

- [ ] **Step 1: Add a test** to `LiteDbManagerLeaseTests.cs`: `Repository_Dispose_ReleasesFile`: construct `new GuildConfigLiteDbRepositoryFactory(NullLoggerFactory.Instance, mgr).Create(new DiscordGuildId(10))` (the factory is `internal`; add `[assembly: InternalsVisibleTo("DiscordBot.ServicesTests")]` to `DiscordBot.Data` via `<InternalsVisibleTo Include="DiscordBot.ServicesTests" />` in `DiscordBot.Data.csproj` if not already present), call `GetSingle()`, dispose the repo → file unlocked. And `GetAll_ReturnsMaterialisedList`: insert 2 docs through the repo, `var all = repo.GetAll().Value; repo.Dispose(); all.Count().Should().Be(2)` (enumerating after dispose must not throw).
- [ ] **Step 2: Run** the Data-related tests → fail to compile.
- [ ] **Step 3: Implement** the changes listed under Files. `dotnet build DiscordBot.Data/DiscordBot.Data.csproj` must succeed; the solution will not yet (Task 3/4 fix consumers).
- [ ] **Step 4: Commit** `feat(data): repositories own a DatabaseLease and are IDisposable`.

### Task 3: Remove the singleton/DI pins in the `DiscordBot` project

**Files:**
- Modify: `DiscordBot.Data/Configuration/ConfigurationExtensions.cs:21-22` — delete the two `.AddTransient(x => x.GetRequiredService<…Factory>().Create())` lines (a disposable transient resolved from the root provider is tracked forever).
- Modify: `DiscordBot/Commands/Interactive/ManageCommandsApplicationCommandHandler.cs` — replace the `_applicationCommandInfoRepository` field with an `IRepositoryStrategy _repositoryStrategy` field; at each use: `using var repo = _repositoryStrategy.GetOrCreateRepository<IApplicationCommandInfoRepository>();`.
- Modify: `DiscordBot/Services/InteractiveCommandHandlerService.cs` — ctor takes `IRepositoryStrategy` instead of `IApplicationCommandInfoRepository`; in `InitializeCommands`: `using var repo = …; var commandInfos = repo.GetAll().Value;`.
- Modify: `DiscordBot/Services/CommandRegistrationService.cs` and `CommandDefinitionRegistrationService.cs` — same substitution; every method that used the field opens `using var repo = …` locally.
- Modify: `DiscordBot/Configuration/ConfigurationExtensions.cs:131-137` — the `Decorate` lambda passes `provider.GetRequiredService<IRepositoryStrategy>()` instead of the repository.
- Modify: `DiscordBot/Commands/Interactive/KillBotCommandHandler.cs` — `_manager.Dispose()` → `_manager.DisposeAll()`.
- Check: `grep -rn "IRuneScapeDropDataRepository\|IApplicationCommandInfoRepository" DiscordBot DiscordBot.Services DiscordBot.Dashboard --include=*.cs` for any other constructor injection of a repository interface; convert each to the strategy pattern.

- [ ] **Step 1: Build** `dotnet build DiscordBot/DiscordBot.csproj` → list of errors is the checklist.
- [ ] **Step 2: Apply** the changes above until `DiscordBot` compiles. (Services project errors are Task 4; if the build stops there, fix Task 4's mechanical sweep first and come back, but keep the commits separate.)
- [ ] **Step 3: Grep guard**: `grep -rnE "^\s*(private|protected|public)?\s*(readonly\s+)?I[A-Za-z]+Repository\s+_" DiscordBot DiscordBot.Services --include=*.cs` must return nothing (no repository fields anywhere).
- [ ] **Step 4: Commit** `refactor: resolve repositories per use instead of pinning them in singletons`.

### Task 4: `using var` sweep in `DiscordBot.Services`

**Files (every `GetOrCreateRepository` / `GetRepository<` site):**
- `DiscordBot.Services/Services/AutomatedDropperService.cs:64,70,81,130`
- `DiscordBot.Services/Services/GroupService.cs:202` and every `GetRepository<…>` call in that file
- `DiscordBot.Services/Services/ClanFundsService.cs:22,33,65,96,120`
- `DiscordBot.Services/Services/GraveyardService.cs:28,50,68,96,101,115,135,148,177,191,196`
- `DiscordBot.Services/Services/CountService.cs`, `PlayerService.cs`, `ConfirmationService.cs` (they use `GetRepository<…>` from `RepositoryService`; grep `GetRepository<`)
- `DiscordBot.Services/Services/BaseGuildConfigurationService.cs:16,29` (`SaveGuildConfig`, `GetGuildConfig`)
- `DiscordBot.Services/Jobs/ConfigurableGuildJob.cs:23`, `HandleRunescapeDropJob.cs:27,90`
- `DiscordBot.Services/Services/RepositoryService.cs:24` — keep; add a `/// <remarks>Caller owns the repository; dispose it (using var).</remarks>` doc comment.

Rules: `var repo = …` → `using var repo = …`. A repository must never be returned from a method or stored in a field. Where a method calls another method that also opens the same guild's repository, that is fine (nested leases share the instance). Where a value read from a repository is returned (`GetSingle().Value`, `GetAll().Value`), it is already materialised (Task 2), so returning it is safe.

- [ ] **Step 1: Build** `dotnet build OrsrDiscordAutomator.sln` — it should already compile (the change is ownership, not types), so the compiler will **not** flag missing `using`s. Use the grep `grep -rn "GetOrCreateRepository\|GetRepository<" DiscordBot.Services --include=*.cs | grep -v "using var" | grep -v RepositoryService.cs` and drive it to empty.
- [ ] **Step 2: Run the full test suite** → 57+ passed, at most the 5 pre-existing failures (`MediaWikiToItemParserTests`, `AutomatedDropperServiceTests`/NSubstitute — compare names with the baseline output, do not fix unrelated tests).
- [ ] **Step 3: Build the Dashboard too** (`dotnet build DiscordBot.Dashboard/DiscordBot.Dashboard.csproj`); it composes the bot and must still compile.
- [ ] **Step 4: Commit** `refactor(services): dispose repositories after use`.

### Task 5: `MemoryReportJob`

**Files:**
- Create: `DiscordBot.Services/Jobs/MemoryReportJob.cs`
- Modify: `DiscordBot.Services/Configuration/QuartzConfiguration.cs` (`ConfigureJobs`: schedule it)

```csharp
public class MemoryReportJob : BaseJob {
    private readonly LiteDbManager _manager;
    public MemoryReportJob(ILogger<MemoryReportJob> logger, LiteDbManager manager) : base(logger) => _manager = manager;
    protected override Task<Result> DoWork() {
        using var proc = System.Diagnostics.Process.GetCurrentProcess();
        Logger.LogInformation("Memory report: workingSet={WorkingSetMb} MB, gcHeap={GcHeapMb} MB, handles={Handles}, openDatabases={OpenDatabases}",
            Environment.WorkingSet / 1_048_576, GC.GetTotalMemory(false) / 1_048_576, proc.HandleCount, _manager.OpenDatabaseCount);
        return Task.FromResult(Result.Ok());
    }
}
```

Schedule: `quartzServices.ScheduleJob<MemoryReportJob>(t => t.WithIdentity("memory-report", "ops").WithSimpleSchedule(s => s.WithIntervalInMinutes(30).RepeatForever()).StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(30))).WithDescription("Logs process memory and open LiteDB count"));`

- [ ] **Step 1: Implement**, build the solution.
- [ ] **Step 2: Commit** `feat(ops): log memory and open database count every 30 minutes`.

### Task 6: Docs

- [ ] Update `docs/codebase/03-data-layer.md` §2 (`LiteDbManager` API: `Lease`, `LeaseCommon`, `OpenDatabaseCount`, `DisposeAll`, `CloseWhenUnused`), §4 (repositories are `IDisposable`, `using var`), §9 (lifecycle now bounded), and the recipe in §7 (constructor takes `DatabaseLease`).
- [ ] Update `docs/codebase/README.md` quick fact about the manager.
- [ ] Set the spec status line to `Status: implemented on feature/hosting-tracker-and-litedb-lifecycle`.
- [ ] Commit `docs: LiteDB lease lifecycle`.
