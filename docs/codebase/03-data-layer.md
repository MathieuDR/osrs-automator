# Data Layer (DiscordBot.Data + DiscordBot.Common data models)

Embedded NoSQL persistence via **LiteDB 5.0.16** (`DiscordBot.Data/DiscordBot.Data.csproj`), targeting net7.0. One `.db` file per Discord guild plus one "common" file shared across all guilds. Consumed by `DiscordBot` (bot process) and `DiscordBot.Dashboard` (Blazor/API host, which hosts the bot as a hosted service in the same process — see `docs/codebase/05-dashboard-wom-connector-and-misc.md`).

This document describes only `DiscordBot.Data` and the data models in `DiscordBot.Common`. It does not cover services/business logic beyond how they obtain repositories.

---

## 1. Storage layout

`DiscordBot.Data/Configuration/LiteDbOptions.cs` — bound from config section `LiteDbOptions` (see `DiscordBot/appsettings.default.json`):

```json
"LiteDbOptions": { "PathPrefix": "db_", "FileSuffix": "def" }
```

`LiteDbManager` (`DiscordBot.Data/LiteDbManager.cs`) builds file names:

```csharp
private string CreateConnectionString(string identifier = "common") =>
    $"{_options.PathPrefix}{identifier}_{_options.FileSuffix}.db";

private string GetGuildFileName(DiscordGuildId guildId) => CreateConnectionString(guildId.ToString());
```

- Common DB file: `{PathPrefix}common_{FileSuffix}.db` → e.g. `db_common_def.db`
- Per-guild DB file: `{PathPrefix}{guildId}_{FileSuffix}.db` → e.g. `db_123456789012345678_def.db`, where `{guildId}` is the numeric `DiscordGuildId.ToString()` (a `StronglyTypedId` wrapping `long`, defined in `DiscordBot.Common/Identities/Identities.cs`).
- Directory is created lazily (`CreateDirectory`) if the resolved path has a directory component and it doesn't exist yet. Files are written relative to the process working directory unless `PathPrefix` includes a directory (e.g. `data/db_`).

**Connection mode:** `CreateDatabase` calls `new LiteDatabase(connectionString, BsonMapper)` with a **plain file path string**, not a full LiteDB connection string with `Connection=Shared`. This means **LiteDB's default `ConnectionType.Direct`** is used: each open `LiteDatabase` instance holds an **exclusive OS file lock** on its `.db` file and keeps its own **in-memory page cache**. Only one `LiteDatabase` instance (in one process) may have a given file open at a time; a second attempt to open the same file (e.g. from the Dashboard process run separately from the bot process) will throw. Within a single process, `LiteDbManager` avoids this by caching one `LiteDatabase` per guild (see §2), so all repositories for a guild share the same open handle.

---

## 2. LiteDbManager (`DiscordBot.Data/LiteDbManager.cs`)

Registered as an **`AddSingleton<LiteDbManager>()`** in `ConfigurationExtensions.UseLiteDbRepositories`.

API:
- `LiteDatabase GetCommonDatabase()` — lazily creates and caches the single common `LiteDatabase` in the field `_commonDatabase`, guarded by `_commonLock`.
- `LiteDatabase GetDatabase(DiscordGuildId guildId)` — looks up `_databases` (a `Dictionary<DiscordGuildId, LiteDatabase>`), creating and caching a new instance per unseen guild, guarded by `_createLock`.
- `void ClearDb()` — disposes the common database and every cached guild database, then clears `_databases`. Sets `_commonDatabase` back to `null` (so it will be recreated on next `GetCommonDatabase()`), but note the per-guild dictionary is cleared entirely, not repopulated.
- `void Dispose()` — calls `ClearDb()`.

**Caching / lifetime:** Once opened, a guild's (or the common) `LiteDatabase` stays open and cached for the lifetime of the singleton (i.e. effectively the app's lifetime) — there is no eviction, TTL, or LRU. The dictionary only grows as new guilds are seen; nothing ever removes a single entry except a full `ClearDb()`/`Dispose()`.

**Who calls `Dispose`/`ClearDb`:** Only `DiscordBot/Commands/Interactive/KillBotCommandHandler.cs`, which calls `_manager.Dispose()` right before `Environment.Exit(0)` (a bot-admin `/kill` slash command), and the test fixture `tests/DiscordBot.ServicesTests/Data/IdentityTests.cs` (test cleanup). In normal operation nothing ever closes a per-guild database until process exit.

**BsonMapper global registrations** (`AddMappers`, run once in the constructor, so run once per singleton, mutating `LiteDB.BsonMapper.Global`):
- Strongly-typed IDs `DiscordUserId`, `DiscordGuildId`, `DiscordChannelId`, `DiscordMessageId`, `DiscordRoleId` are each registered via `BsonMapper.RegisterType(...)`, serializing to/from the wrapped `long`/`Int64` BSON value.
- `AddDictMapper<TIdentity, TObject>(...)` (public static helper) registers custom BSON (de)serialization for `Dictionary<TIdentity, TObject>` where the key is one of the identity types (LiteDB can't natively use non-primitive dictionary keys). Registered combinations: `Dictionary<DiscordRoleId, AuthorizationRoles>`, `Dictionary<DiscordUserId, AuthorizationRoles>`, `Dictionary<DiscordUserId, List<Shame>>`, `Dictionary<DiscordUserId, EndpointId>`. **Any new model with a `Dictionary<TIdentity, TValue>` property must add a matching `AddDictMapper` call here** or LiteDB will fail to (de)serialize it (see `CommandRoleConfig`, `DropperGuildConfiguration.UserEndpoints`, `AutomatedMessagesConfig.ChannelJobs` which uses `Dictionary<JobType, ChannelJobConfiguration>` — `JobType` is a plain enum so it needs no custom mapper).
- `BsonMapper` is exposed as a mutable public property (`public BsonMapper BsonMapper { get; set; }`), initialized to `BsonMapper.Global` — i.e. it is process-wide global state, not scoped per database.

---

## 3. Repository stack

Interfaces (`DiscordBot.Data/Interfaces/`):
- `IRepository` — empty marker interface.
- `IRepository<T> : IRepository where T : BaseModel, new()` — `GetAll`, `Get(ObjectId)`, `Insert`, `Update`, `UpdateOrInsert`, `Delete`, `BulkInsert`, `BulkUpdateOrInsert`. All return `FluentResults.Result`/`Result<T>`.
- `IRecordRepository<T> : IRepository where T : BaseRecord, new()` — same CRUD surface minus bulk ops (record-based models).
- `ISingleRecordRepository<T> : IRecordRepository<T>` — adds `Result<T> GetSingle()`, for collections that are expected to hold exactly one document (per-guild singleton configuration documents).

Base implementations (`DiscordBot.Data/Repository/`), all `internal abstract`:
- `BaseLiteDbRepository<T> : IRepository<T>` — wraps a `LiteDatabase`, exposes `abstract string CollectionName`, implements all CRUD via `LiteDatabase.GetCollection<T>(CollectionName)`. `UpdateOrInsert` branches on `entity._id is not null`.
- `BaseRecordLiteDbRepository<T> : IRecordRepository<T>` — same shape for `BaseRecord`-derived (record) models; branches on `entity.Id is not null`.
- `BaseSingleRecordLiteDbRepository<T> : BaseRecordLiteDbRepository<T>, ISingleRecordRepository<T>` — adds `GetSingle() => GetAll().Value.FirstOrDefault()` (i.e. no query filter — it truly assumes the collection holds ≤1 document).

### Model → Repository → Interface → Factory → Collection table

| Model | Repository | Interface | Factory | Collection name | Scope |
|---|---|---|---|---|---|
| `GuildConfig` | `GuildConfigRepository` | `IGuildConfigRepository` | `GuildConfigLiteDbRepositoryFactory` | `"guildConfig"` | per-guild |
| `Player` | `PlayerRepository` | `IPlayerRepository` | `PlayerLiteDbRepositoryFactory` | `"players"` | per-guild |
| `UserCountInfo` | `UserCountInfoRepository` | `IUserCountInfoRepository` | `UserCountInfoLiteDbRepositoryFactory` | `"guildUserCounts"` | per-guild |
| `AutomatedJobState` | `AutomatedJobStateRepository` | `IAutomatedJobStateRepository` | `AutomatedJobStateLiteDbRepositoryFactory` | `"guildJobState"` | per-guild |
| `ClanFunds` | `ClanFundsRepository` | `IClanFundsRepository` | `ClanFundsLiteDbRepositoryFactory` | `"ClanFunds"` | per-guild |
| `Graveyard` | `GraveyardRepository` | `IGraveyardRepository` | `GraveyardLiteDbRepositoryFactory` | `"graveyard"` | per-guild |
| `Confirmation` | `ConfirmationRepository` | `IConfirmationRepository` | `ConfirmationLiteDbRepositoryFactory` | `"confirmations"` | per-guild |
| `ConfirmationConfiguration` | `ConfirmConfigurationRepository` | `IConfirmConfigurationRepository` | `ConfirmationConfigurationLiteDbRepositoryFactory` | `"confirmationConfiguration"` | per-guild |
| `SelfCountConfiguration` | `SelfCountConfigurationRepository` | `ISelfCountConfigurationRepository` | `SelfCountConfigurationLiteDbRepositoryFactory` | `"selfCountConfiguration"` | per-guild |
| `DropperGuildConfiguration` | `RunescapeDropperGuildConfigurationRepository` | `IRunescapeDropperGuildConfigurationRepository` | `RunescapeDropperGuildConfigurationRepositoryFactory` | `"runescapeDropperGuildConfiguration"` | per-guild |
| `SelfCountConfiguration` (again, see §9) | `ItemsRepository` | `IItemsRepository` | `ItemsLiteDbRepositoryFactory` | `"items"` | per-guild |
| `ApplicationCommandInfo` | `ApplicationCommandInfoRepository` | `IApplicationCommandInfoRepository` | `CommandInfoRepositoryFactory` | `nameof(ApplicationCommandInfo)` = `"ApplicationCommandInfo"` | **common** |
| `RunescapeDropData` | `RuneScapeDropDataRepository` | `IRuneScapeDropDataRepository` | `RunescapeDropDataRepositoryFactory` | `"RunescapeDropRecords"` | **common** |

Only two factories are "common"-scoped (`RequiresGuildId => false`): `CommandInfoRepositoryFactory` and `RunescapeDropDataRepositoryFactory`. Every other factory has `RequiresGuildId => true` and its `Create()` (no-guild) overload throws `NotImplementedException`; conversely, common-scoped factories throw `NotImplementedException` from `Create(DiscordGuildId)`.

Some repositories add query methods beyond the base CRUD surface (declared on their own interface, not on the generic base interfaces): `IPlayerRepository.GetByDiscordId/GetPlayerByOsrsAccount`, `IGuildConfigRepository.GetSingle()` (note: `GuildConfig` derives from `BaseGuildModel`/`BaseModel`, not `BaseRecord`, so it does **not** get `ISingleRecordRepository`'s `GetSingle()` — `IGuildConfigRepository` declares its own, implemented as `GetCollection().FindAll().SingleOrDefault()`, which throws if more than one `GuildConfig` document ever exists in a guild's db), `IGraveyardRepository` (shame-management helpers operating on the single `Graveyard` document), `IConfirmationRepository.GetUnconfirmedByMessageId`, `IApplicationCommandInfoRepository.GetByCommandName`, `IUserCountInfoRepository.GetByDiscordUserId`.

**Interface/implementation mismatch to note:** `RuneScapeDropDataRepository` (concrete class) implements `HasActiveDrop(EndpointId)`, `GetActive(EndpointId)`, and `CloseActive(EndpointId)` in addition to the `DiscordUserId` overloads, but `IRuneScapeDropDataRepository` only declares the `DiscordUserId` overloads of `GetActive`/`CloseActive` and does not declare `HasActiveDrop` at all. Any caller holding the repository as `IRuneScapeDropDataRepository` (which is how it's always obtained via `IRepositoryStrategy`/DI) cannot call the `EndpointId` overloads or `HasActiveDrop` — those are effectively dead/inaccessible through normal consumption paths.

---

## 4. Factories + RepositoryStrategy

`DiscordBot.Data/Factories/BaseLiteDbRepositoryFactory<TInterface, TConcrete>` (`internal abstract`) implements `IRepositoryFactory<TInterface>` (`DiscordBot.Data/Factories/IRepositoryFactory.cs`):

```csharp
public bool AppliesTo(Type type, bool requiresGuildId) =>
    typeof(TInterface).IsAssignableFrom(type) && requiresGuildId == RequiresGuildId;
```

Each concrete factory (e.g. `GuildConfigLiteDbRepositoryFactory`) overrides `RequiresGuildId` and `Create(DiscordGuildId)`/`Create()`, and constructs a **brand-new repository object per call**, e.g.:

```csharp
public override IGuildConfigRepository Create(DiscordGuildId guildId) =>
    new GuildConfigRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
```

`LiteDbManager.GetDatabase(guildId)` returns the cached `LiteDatabase` (opening it on first use), so the repository object is cheap/throwaway but the underlying `LiteDatabase` connection is long-lived and shared.

`RepositoryStrategy` (`DiscordBot.Data/Strategies/RepositoryStrategy.cs`) holds an `IRepositoryFactory[]` and resolves by linear scan:

```csharp
public T GetOrCreateRepository<T>(DiscordGuildId guildId) =>
    (T)_factories.FirstOrDefault(f => f.AppliesTo(typeof(T), true))?.Create(guildId)
      ?? throw new InvalidOperationException($"{typeof(T)} not registered");
```

Every call to `GetOrCreateRepository<T>(...)` creates a new repository instance (`factory.Create(...)` → `new XyzRepository(...)`). **There is no repository-level caching** — only the `LiteDatabase` connection is cached (in `LiteDbManager`).

**DI lifetimes** (`DiscordBot.Data/Configuration/ConfigurationExtensions.cs`, method `UseLiteDbRepositories`):
- `MigrationManager` — singleton
- `LiteDbManager` — singleton
- Every `*LiteDbRepositoryFactory` (e.g. `GuildConfigLiteDbRepositoryFactory`, `PlayerLiteDbRepositoryFactory`, …) — **transient**
- `IRepositoryStrategy` — singleton, constructed via a factory lambda that resolves each `*LiteDbRepositoryFactory` **once** from the root provider at first resolution of `IRepositoryStrategy` and captures them in an array passed to `new RepositoryStrategy(...)`. **Effect:** even though each factory is registered `AddTransient`, because `IRepositoryStrategy` is a singleton that resolves and holds onto them exactly once, those factory *instances* behave as de-facto singletons for the lifetime of the app — new instances are never created after container build. The transient registration only matters if something else resolves a factory type directly from DI (nothing in the codebase does, except the two special-cased common repositories below).
- Two repository interfaces are also registered directly as transient services, resolved from their factories eagerly: `IApplicationCommandInfoRepository` (via `CommandInfoRepositoryFactory.Create()`) and `IRuneScapeDropDataRepository` (via `RunescapeDropDataRepositoryFactory.Create()`) — these can be injected directly by constructor as well as obtained via `IRepositoryStrategy.GetOrCreateRepository<T>()`.

`UseLiteDbRepositories` is called from both `DiscordBot/Program.cs` and `DiscordBot.Dashboard/StartupHelper.cs` — each builds its own `IServiceCollection`/root provider, so if bot and dashboard were ever run as **separate processes** against the same `LiteDbOptions.PathPrefix`, each would have its own singleton `LiteDbManager` and would attempt to open the same `.db` files independently — which conflicts with `Direct` connection mode's exclusive file lock (see §1). In the repo's own run configs, the Dashboard hosts the bot as a hosted service in-process, so this collision is avoided there — but it is not structurally prevented if the two are ever split into separate processes/deployments pointed at the same data directory.

---

## 5. Models (`DiscordBot.Common/Models/Data/`)

### Base types (`Models/Data/Base/`)
- `BaseModel` (class): `[BsonId] ObjectId _id`, `DiscordUserId CreatedByDiscordId`, `DateTime CreatedOn` (set in ctor to `DateTime.Now`, i.e. **local time, not UTC**). Has `IsValid()` (throws `ValidationException` via a protected `ValidationDictionary`) and `ToDictionary()` (reflection-based, primitives/strings only).
- `BaseGuildModel : BaseModel`: adds `DiscordGuildId GuildId`.
- `BaseRecord` (record): `[BsonId] ObjectId Id`, `DateTime CreatedOn` (init-only, defaults to `DateTime.Now`).
- `BaseGuildRecord : BaseRecord` (record): adds `DiscordGuildId GuildId`, `DiscordUserId CreatedById`.

Note the inconsistent Id property naming: `BaseModel._id` (lowercase, `[BsonId]`, mutable) vs `BaseRecord.Id` (PascalCase, `[BsonId]`, init-only) — this is why there are two parallel repository/interface hierarchies (`IRepository<T>`/`BaseLiteDbRepository<T>` for `BaseModel` types, `IRecordRepository<T>`/`BaseRecordLiteDbRepository<T>` for `BaseRecord` types).

### Configuration (`Models/Data/Configuration/`)
- **`GuildConfig : BaseGuildModel`** — the closest thing to "registered guild" state. Fields: `WomGroupId` (int, Wise Old Man group id), `WomVerificationCode`, `WomGroup` (cached `WiseOldManConnector.Models.Output.Group`), `Timezone` (string), `AutoAddNewAccounts` (bool), `AutomatedMessagesConfig` (nested, defaults to `new()`), `CountConfig` (nullable, no default — see `CountConfig`), `CommandRoleConfig` (nested, defaults to `new()`). One document expected per guild db (see `IGuildConfigRepository.GetSingle()` in §3).
- `AutomatedMessagesConfig` — `Dictionary<JobType, ChannelJobConfiguration> ChannelJobs`.
- `ChannelJobConfiguration` — `bool IsEnabled`, `DiscordChannelId ChannelId`, `DiscordGuildId GuildId`. Per-job (per `JobType`) output-channel + on/off toggle.
- `AutomatedJobState : BaseGuildModel` — `Achievement LastPrintedAchievement` (from `WiseOldManConnector.Models.Output`); tracks last WOM achievement announced per guild, to avoid re-announcing.
- `CommandRoleConfig` — `Dictionary<DiscordRoleId, AuthorizationRoles> RoleIds`, `Dictionary<DiscordUserId, AuthorizationRoles> UserIds`; per-guild command authorization overrides.
- `ApplicationCommandInfo : BaseRecord` — `CommandName`, `Hash` (uint, for change detection), `IsGlobal`, `List<DiscordGuildId> RegisteredGuilds`. Lives in the **common** db; tracks which slash commands have been registered where.

### Counting (`Models/Data/Counting/`)
- `CountConfig` — `DiscordChannelId OutputChannelId`, `List<CountThreshold> _thresholds` (BSON-mapped field name `"Thresholds"` via `[BsonField("Thresholds")]`, exposed read-only as `Thresholds`). Embedded inside `GuildConfig`.
- `CountThreshold` — `CreatorId`, `CreatorUsername`, `Threshold` (int), `GivenRoleId` (nullable `DiscordRoleId`), `Name`.
- `Count` — one count-change event: `Additive` (int), `RequestedBy`, `RequestedDiscordTag`, `RequestedOn` (`DateTimeOffset`), `Reason`.
- `UserCountInfo : BaseModel` — `DiscordId`, `CurrentCount` (computed: `CountHistory.Sum(x => x.Additive)`, **not stored**), `List<Count> CountHistory`.
- `SelfCountConfiguration : BaseGuildRecord` — `List<Item> Items`, nullable `RequestChannel`. See §9 for the duplicate-repository oddity around this model.

### ClanFunds (`Models/Data/ClanFunds/`)
- `ClanFunds : BaseGuildRecord` — `ChannelId`, `DonationLeaderBoardChannel`, `DonationLeaderBoardMessage`, `List<ClanFundEvent> Events`; several derived (non-persisted, computed) totals: `TotalFunds`, `TotalDonations`, `HighestDonation`, `TotalRefunds`, per-player donation totals.
- `ClanFundEvent` (record) — `Id` (Guid), `PlayerId`, `CreatorId`, `Reason`, `PlayerName`, `Amount` (long), `EventType` (`ClanFundEventType`), `EventDate`.
- `ClanFundEventType` (enum) — `Deposit, Withdraw, Donation, Refund, Other, System`.

### Confirmation (`Models/Data/Confirmation/`)
- `Confirmation : BaseRecord` — `ConfirmMessage` (message id), `RequestedBy`, `ConfirmedBy` (nullable), `ConfirmCommand` (`IConfirmCommand`, polymorphic — stored as whatever concrete implementation was passed in), `IsConfirmed`, `IsDenied`.
- `ConfirmationConfiguration : BaseGuildRecord` — `DiscordChannelId ConfirmationChannel`.

### Drops (`Models/Data/Drops/`)
- `DropperGuildConfiguration : BaseGuildRecord` — `IsEnabled`, `DisabledUsers`, `MasterConfiguration` (`DropperConfiguration`), `Dictionary<DiscordChannelId, List<DropperConfiguration>> ChannelConfigurations`, `Dictionary<DiscordUserId, EndpointId> UserEndpoints`.
- `DropperConfiguration` (record, not itself persisted top-level, embedded) — allow/block lists (users, items, sources, player builds), min GE/HA value thresholds, `AlwaysSendCollectionLogItems`.
- `RunescapeDropData : BaseRecord` — **common** db. `UserId`, `Endpoint` (`EndpointId`), `Drops` (`IEnumerable<RunescapeDrop>`), `IsHandled` (bool — marks whether the pending drop batch has been closed out), `GuildsMessaged`, plus computed `TotalValue`/`TotalHaValue`/`PlayerDrops`.

### Graveyard (`Models/Data/Graveyard/`)
- `Graveyard : BaseGuildRecord` — `List<DiscordUserId> OptedInUsers`, `Dictionary<DiscordUserId, List<Shame>> Shames`. One document per guild (accessed via `ISingleRecordRepository.GetSingle()`).
- `Shame` (record) — `Id` (Guid), `ShamedAt`, `Location` (`ShameLocation` enum), `MetricLocation` (nullable `MetricType` from WiseOldManConnector), `ShamedBy`, `ImageUrl`.

### Items (`Models/Data/Items/`)
- `Item : BaseRecord` — `Name`, `Synonyms` (list, auto-includes `Name` itself), `Value` (int), `SplitValue` (nullable int), computed `Splittable`.

### PlayerManagement (`Models/Data/PlayerManagement/`)
- `Player : BaseGuildModel` — `DiscordUserId` (computed alias for `CreatedByDiscordId`), `WiseOldManDefaultPlayerId`, `DefaultPlayerUsername`, `Nickname`, `List<WiseOldManConnector.Models.Output.Player> CoupledOsrsAccounts`, `EnforceNameTemplate`. Overrides `IsValid()` to require a non-empty `DefaultPlayerUsername` and non-negative WOM id.

### Enums (`Models/Enums/`)
- `AuthorizationRoles` — `[Flags]` bitmask (`BotOwner=1, BotAdmin=2, BotModerator=4, ClanOwner=8, ClanAdmin=16, ClanModerator=32, ClanEventHost=64, ClanEventParticipant=128, ClanMember=256, ClanGuest=512, None=1024`).
- `BotPermissions` — `[Flags]` (`None=1, EventManager=2, CompetitionManager=4`) — appears unrelated to persistence directly; not stored on any model read in this scope.
- `JobType` — `Achievements=1, GroupUpdate=20, MonthlyTop=40, MonthlyTopGains=80`; keys `AutomatedMessagesConfig.ChannelJobs`.
- `ShameLocation` — `Other, MetricType, Wildy`.

### Identities (`DiscordBot.Common/Identities/Identities.cs`)
`StronglyTypedId`-generated structs wrapping `long`: `DiscordUserId`, `DiscordGuildId`, `DiscordChannelId`, `DiscordMessageId`, `DiscordRoleId` (all constructible from `ulong`, expose `UlongValue`), plus `EndpointId` (`Guid`-backed). Each has a matching `BsonMapper.RegisterType` in `LiteDbManager.AddMappers()` (§2) — **a new strongly-typed id type must be registered there too, or LiteDB will fail/fall back to reflection-based (de)serialization of the struct's internal fields.**

---

## 6. Migrations (`DiscordBot.Data/Repository/Migrations/`)

- `IMigration` — `int Version`, `void Up(LiteDatabase)`, `void Down(LiteDatabase)`.
- `BaseMigration : IMigration` (abstract) — wraps `DoUp`/`DoDown` in `database.BeginTrans()`/`Commit()` and sets `database.UserVersion = Version` (or `Version - 1` on `Down`). LiteDB's `UserVersion` is a built-in per-file integer used to track schema version.
- `MigrationManager` (singleton) — constructed with a hardcoded list: currently only `new MigrationToBetterCountModels(...)` (`Version => 1`). `Validate()` (run in ctor) checks for duplicate version numbers and that every version from 1 up to `CurrentMigration` (= max registered version) exists — **adding migration N+2 while skipping N+1 throws at startup.**
- `Migrate(LiteDatabase, int? migrateTo = null)` — compares `liteDatabase.UserVersion` to the target version (default = `CurrentMigration`, i.e. latest) and runs `Up` (or `Down`, if `migrateTo` is lower than current — no caller currently passes a lower `migrateTo`) migrations one version at a time until they match.

**When migrations run:** `LiteDbManager.CreateDatabase(connectionString)` calls `_manager.Migrate(liteDatabase)` immediately after constructing every new `LiteDatabase` — i.e. **on every first-open of every guild db and the common db** (not on every operation — only once per process lifetime per db, since the `LiteDatabase` is then cached). A `LogContext.PushProperty("db", connectionString)` scopes migration log lines to the db file name.

**Existing migration:** `MigrationToBetterCountModels` (version 1) — renames `guildConfig.CountConfig._tresholds` → `Thresholds` and each threshold's `Treshold` field → `Threshold` (typo fix), operating directly on raw `BsonDocument`s via `database.GetCollection("guildConfig")` (untyped). `DoDown` reverses it and is annotated "untested".

**To add a new migration:** create a class deriving `BaseMigration` with the next sequential `Version`, implement `DoUp`/`DoDown` using untyped `database.GetCollection(name)` + `BsonDocument` manipulation (since typed collections would use the *current* C# model shape, not the old on-disk shape), and add `new YourMigration(loggerFactory.CreateLogger<YourMigration>())` to the `_migrations` list in `MigrationManager`'s constructor.

---

## 7. Recipe: adding a new persisted setting / collection

### 7a. Add a field to `GuildConfig` (simplest case — no new collection)
1. Add the property to `DiscordBot.Common/Models/Data/Configuration/GuildConfig.cs`. Give it a sensible default (LiteDB will simply omit/default missing fields on old documents — no migration is strictly required for additive, backward-compatible changes).
2. If the new property's type is a `Dictionary<TIdentity, TValue>` keyed by one of the strongly-typed ids, add the corresponding `LiteDbManager.AddDictMapper<TIdentity, TValue>(...)` call in `LiteDbManager.AddMappers()` (§2) — otherwise BSON (de)serialization of that dictionary will fail or silently misbehave.
3. If the change is not safely additive (e.g. renaming/restructuring an existing field), add a migration (§6) instead of relying on default values.
4. Read/write it through the existing `IGuildConfigRepository` (`GetSingle()` / `UpdateOrInsert(GuildConfig)`), typically via `BaseGuildConfigurationService.GetGuildConfig(...)`/`SaveGuildConfig(...)` (`DiscordBot.Services/Services/BaseGuildConfigurationService.cs`) — no new repository needed.

### 7b. Add a brand-new per-guild collection (model based on `BaseGuildModel`)
Using `AutomatedJobStateRepository` as the template (`DiscordBot.Data/Repository/AutomatedJobStateRepository.cs` + `DiscordBot.Data/Factories/AutomatedJobStateLiteDbRepositoryFactory.cs`):

1. **Model** — `DiscordBot.Common/Models/Data/<Area>/YourModel.cs`:
   ```csharp
   public class YourModel : BaseGuildModel {
       public YourField YourProperty { get; set; }
   }
   ```
   (or `: BaseGuildRecord` if you prefer an immutable record — then base your repository on `BaseRecordLiteDbRepository`/`BaseSingleRecordLiteDbRepository` instead, per §3.)

2. **Interface** — `DiscordBot.Data/Interfaces/IYourModelRepository.cs`:
   ```csharp
   public interface IYourModelRepository : IRepository<YourModel> { }
   ```

3. **Repository** — `DiscordBot.Data/Repository/YourModelRepository.cs`:
   ```csharp
   internal class YourModelRepository : BaseLiteDbRepository<YourModel>, IYourModelRepository {
       public YourModelRepository(ILogger<YourModelRepository> logger, LiteDatabase database) : base(logger, database) { }
       public override string CollectionName => "yourModelCollectionName";
   }
   ```

4. **Factory** — `DiscordBot.Data/Factories/YourModelLiteDbRepositoryFactory.cs`:
   ```csharp
   internal class YourModelLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IYourModelRepository, YourModelRepository> {
       public YourModelLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }
       public override bool RequiresGuildId => true;
       public override IYourModelRepository Create(DiscordGuildId guildId) =>
           new YourModelRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
       public override IRepository Create() => throw new NotImplementedException();
   }
   ```

5. **Wire into DI** — in `DiscordBot.Data/Configuration/ConfigurationExtensions.cs`, add `.AddTransient<YourModelLiteDbRepositoryFactory>()` and add `x.GetRequiredService<YourModelLiteDbRepositoryFactory>()` to the array passed into `new RepositoryStrategy(...)`.

6. **Consume** — inject `IRepositoryStrategy` into a service/job and call `RepositoryStrategy.GetOrCreateRepository<IYourModelRepository>(guildId)`.

### 7c. Add a new common-db collection
Same as 7b, but base the model on plain `BaseModel`/`BaseRecord` (no `GuildId`), set `RequiresGuildId => false` in the factory, override `Create()` (not `Create(DiscordGuildId)`) to call `LiteDbManager.GetCommonDatabase()`, and have `Create(DiscordGuildId)` throw `NotImplementedException` — mirror `CommandInfoRepositoryFactory`/`RunescapeDropDataRepositoryFactory`. Optionally also register the interface directly as transient (`.AddTransient(x => x.GetRequiredService<YourFactory>().Create())`) if you want constructor injection in addition to `IRepositoryStrategy`.

---

## 8. Enumerating all guilds / GuildConfigs

There is **no repository method that lists all guild ids or all `GuildConfig` documents** across the data directory (no directory scan of `db_*_def.db` files, and `IGuildConfigRepository` has no "get all guilds" query — `GetAll()` from the base `IRepository<T>` would only return `GuildConfig` documents from whichever single guild's already-opened database it's called against).

Guild enumeration is instead done via the **live Discord gateway connection**: `IDiscordService.GetGuilds()` (`DiscordBot/Services/DiscordService.cs`) returns `_client.Guilds` (a `DiscordSocketClient`, from Discord.Net) mapped to `Guild` DTOs — guilds the bot is *currently connected to*, not guilds that have (or ever had) a database file. `BaseGuildJob.DoWork()` (`DiscordBot.Services/Jobs/BaseGuildJob.cs`) is the main consumer of this pattern: it calls `DiscordService.GetGuilds()`, then fans out `DoWorkForGuild(guild)` per guild in parallel; `ConfigurableGuildJob.DoWorkForGuild` (`DiscordBot.Services/Jobs/ConfigurableGuildJob.cs`) then does `RepositoryStrategy.GetOrCreateRepository<IGuildConfigRepository>(guild.Id).GetSingle()` per guild to fetch that guild's config.

**Implication:** a guild db file that exists on disk but whose guild the bot is no longer a member of (e.g. bot was kicked) is invisible to this enumeration path and becomes an orphaned file — nothing in the codebase currently detects or cleans these up.

---

## 9. Resource lifecycle observations (factual, no fixes attempted here)

- **Per-guild `LiteDatabase` instances are opened once and never closed** except via the admin `/kill` command (`KillBotCommandHandler` → `LiteDbManager.Dispose()`) or process exit. `_databases` (the guild cache dict in `LiteDbManager`) only grows; there is no per-entry disposal/eviction, so a long-running bot that is added to many guilds accumulates that many open file handles + LiteDB in-memory page caches for the process's entire lifetime.
- **Repositories are created fresh on every call** (`RepositoryStrategy.GetOrCreateRepository<T>()` → `factory.Create(...)` → `new XyzRepository(...)`), but they are lightweight wrappers (logger + `LiteDatabase` reference) with no `IDisposable` implementation and no unmanaged resources of their own — the actual resource (the open `LiteDatabase`/file handle) lives only in `LiteDbManager`'s cache, so this per-call allocation is not itself a leak, just churn.
- **`IRepositoryStrategy` is a singleton that captures factory instances once at first resolution.** Because the factories close over the singleton `LiteDbManager` (constructor-injected once), and never get re-resolved, this is consistent — but it does mean the `AddTransient` lifetime declared for the factories is misleading; check §4.
- **Duplicate/dead repository around `SelfCountConfiguration`:** both `ItemsRepository`/`IItemsRepository`/`ItemsLiteDbRepositoryFactory` (collection `"items"`) and `SelfCountConfigurationRepository`/`ISelfCountConfigurationRepository`/`SelfCountConfigurationLiteDbRepositoryFactory` (collection `"selfCountConfiguration"`) wrap the **same model**, `SelfCountConfiguration`. Only `ISelfCountConfigurationRepository` is actually consumed (by `DiscordBot.Services/Services/CountService.cs`); `IItemsRepository`/`ItemsRepository` is registered in DI (`ConfigurationExtensions`) but grep shows no consumer anywhere in `DiscordBot`/`DiscordBot.Services`/`DiscordBot.Dashboard` — it appears to be dead code / a copy-paste leftover (its name doesn't match the model it wraps, `Item` is a different, unrelated model). An AI agent should not assume `IItemsRepository` is the repository for the `Item` model.
- **Interface/implementation surface mismatch** on `IRuneScapeDropDataRepository` vs `RuneScapeDropDataRepository` — see §3. Extending the interface (rather than only the concrete class) is required to expose `HasActiveDrop`/`EndpointId`-overloads to DI-injected consumers.
- **Two separate DI root compositions** (`DiscordBot/Program.cs` and `DiscordBot.Dashboard/StartupHelper.cs`) both call `UseLiteDbRepositories`, each building an independent singleton `LiteDbManager`. Currently the repo's dashboard run configuration hosts the bot in the same process (avoiding file-lock conflicts), but nothing in `LiteDbManager`/`LiteDbOptions` prevents misconfiguration where two separate processes point at the same `PathPrefix` — Direct-mode LiteDB would then fail to open a file already locked by the other process.
- `BaseModel.CreatedOn` / `BaseRecord.CreatedOn` are set via `DateTime.Now` (local server time), not `DateTime.UtcNow` — timestamps are not timezone-normalized across environments.
- `IGuildConfigRepository.GetSingle()` uses `.SingleOrDefault()` (throws if the `"guildConfig"` collection ever ends up with more than one document for a guild — there is no guard preventing a second `Insert` from creating a duplicate; callers use `UpdateOrInsert`, which is safe as long as they always fetch-then-save the same in-memory instance).

---

## 10. Gotchas for an AI editing this code

1. **`GuildId.ToString()` is the file discriminator.** Never change `DiscordGuildId`'s formatting/underlying type without a data-migration plan — it directly determines the on-disk file name (§1), and existing `.db` files will become unreachable if the format changes.
2. **New identity-typed fields need a `BsonMapper.RegisterType`/`AddDictMapper` call in `LiteDbManager.AddMappers()`.** This is easy to forget when adding a new strongly-typed id or a new `Dictionary<TIdentity, TValue>` shape — LiteDB will otherwise fail or fall back to unreliable reflection-based mapping.
3. **`BsonMapper.Global` is mutated as global, static, process-wide state** in the `LiteDbManager` constructor — not scoped to the instance. Multiple `LiteDbManager` instances (e.g. in tests) re-run `AddMappers()`, but `RegisterType`/dict-mapper registrations are idempotent overwrites, not additive duplicates, so this is safe but worth knowing when writing tests that construct `LiteDbManager` more than once.
4. **Repositories are always obtained through `IRepositoryStrategy.GetOrCreateRepository<TInterface>([guildId])`, never `new`'d directly** by services/jobs (`RepositoryService.GetRepository<T>`, `BaseGuildConfigurationService`, `RepositoryJob`, etc. all funnel through it). The resolution is purely by **exact interface type match** (`typeof(TInterface).IsAssignableFrom(type)`) plus whether a `guildId` was supplied — if you add a new repository interface that extends an *existing* repository interface (rather than the raw `IRepository`/`IRecordRepository<T>`/`ISingleRecordRepository<T>` bases), make sure your factory's `AppliesTo` still resolves unambiguously; the strategy uses `FirstOrDefault`, so factory order in the registration array matters if multiple factories could match a requested type.
5. **`Create()` vs `Create(DiscordGuildId)` — exactly one is implemented per factory, the other throws `NotImplementedException`.** Calling `GetOrCreateRepository<T>()` for a guild-scoped repository (or vice versa) throws at runtime, not compile time — there is no static guarantee tying a repository interface to whether it needs a guild id.
6. **Never call `.Dispose()` directly on a `LiteDatabase` obtained from `LiteDbManager.GetDatabase`/`GetCommonDatabase`** in production code — it is a shared, cached instance; disposing it out from under other callers leaves a disposed object cached in `LiteDbManager`'s dictionary until `ClearDb()` is called. (The unit test `IdentityTests` does this deliberately inside `using` blocks and then calls `_dbManager.ClearDb()` immediately after to reset the cache — don't copy that pattern into non-test code without the accompanying `ClearDb()`.)
7. **Migrations operate on raw, untyped `BsonDocument`s**, not the current C# model — because by definition a migration exists to transform data from an *older* shape that the current model no longer represents. Don't "simplify" a migration to use `GetCollection<CurrentModel>()`.
8. **Migrations run once per db file, at first-open, driven by the file's own `UserVersion`** — not on every operation, and not centrally/eagerly for all guild files at startup (a guild db is only opened, and thus only migrated, the first time some code path calls `GetDatabase(guildId)` for it in that process's lifetime).
9. **There is no cross-guild query capability.** Any feature needing "all guilds" data must iterate live `IDiscordService.GetGuilds()` and query each guild's db individually (§8) — there's no aggregate index, and no directory scan of `.db` files exists to fall back on.
10. **`FluentResults.Result`/`Result<T>` is the return convention throughout the repository layer** — don't introduce exceptions-as-control-flow or nullable-returning methods inconsistent with the rest of the stack; existing consumers pattern-match on `IsFailed`/`.Value`/`.ValueOrDefault`.
11. **LiteDB `Direct` connection mode = exclusive file lock.** Don't assume multiple processes (or multiple `LiteDbManager` instances within one process pointed at the same `PathPrefix`) can safely share a `.db` file concurrently — they can't, without switching the connection string to `Connection=Shared` (which the code does not currently do, and which has different performance/consistency tradeoffs).
