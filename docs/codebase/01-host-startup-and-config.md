# Host, Startup, and Configuration

Scope: the `DiscordBot` console host, its DI/configuration wiring, and the Discord gateway client lifecycle. Solution: `OrsrDiscordAutomator.sln` (.NET 7, `LangVersion 10`).

## 1. Solution layout

| Project | Purpose | Status |
|---|---|---|
| `DiscordBot/DiscordBot.csproj` | The actual bot: console host, `Bot : BackgroundService`, command handlers, DI wiring for the whole app. Ships in the Docker image. | Active |
| `DiscordBot.Common/DiscordBot.Common.csproj` | Shared DTOs/models, `Configuration/*` option classes, identity value types (`DiscordGuildId`, `DiscordUserId`, etc.). | Active |
| `DiscordBot.Services/DiscordBot.Services.csproj` | Business services (`IGroupService`, `IPlayerService`, `IAutomatedDropperService`, …), Quartz job configuration, Refit client for the OSRS wiki. | Active |
| `DiscordBot.Data/DiscordBot.Data.csproj` | LiteDB persistence: `LiteDbManager`, per-repository factories, migrations. | Active |
| `WiseOldManConnector/WiseOldManConnector.csproj` | Typed client for the Wise Old Man API. | Active |
| `WiseOldManConnectorTests`, `tests/DiscordBot.ServicesTests`, `tests/WebAppTests` | xUnit test projects. | Active (see §7 for pass/fail state) |
| `WOMReader/WOMReader.csproj` | Standalone console exe, separate from the bot, for ad-hoc reads against the WOM API; has its own Rider `.run` config. | Standalone/exploratory, not referenced by the bot or Docker build. |
| `DiscordBot.Dashboard/DiscordBot.Dashboard.csproj` | Blazor Server + ASP.NET Core web app (`Microsoft.NET.Sdk.Web`) with pages/controllers for guild/dropper configuration, and it **also runs the bot** — `Program.cs` calls `builder.Services.AddHostedService<DiscordBot.Bot>()`, listens on `http://*:5829`. | Real, non-trivial code, but **not part of the Docker build** (only `DiscordBot.csproj` is published) — a second, parallel entry point. |
| `src/DiscordBot.Components/DiscordBot.Components.csproj` | Razor class library referenced by the Dashboard. | Placeholder/scaffold — only template content (`Component1.razor`, `ExampleJsInterop.cs`). |
| `DataLayer/DataLayer.csproj` | Contains only `Class1.cs` (empty class). | Dead/placeholder, not referenced anywhere. |
| `ServiceLayer/ServiceLayer.csproj` | Contains only `Class1.cs` (empty class). | Dead/placeholder, not referenced anywhere. |

The `.sln` also has solution folders `src`, `tests`, `etc` (the latter holds `README.md`/`.gitignore` as solution items only).

## 2. Startup sequence

Entry point: **`DiscordBot/Program.cs`**, class `Program`, `Main()` → `EntryPointAsync()` (`<GenerateProgramFile>false</GenerateProgramFile>` in the csproj; `Program` is `internal`).

```
Main() -> EntryPointAsync()
  1. BuildConfig()               // IConfiguration from appsettings.json (+ optional env-named file)
  2. ConfigureServices(config)   // builds an IServiceCollection, calls ConfigureSerilogger(), returns IServiceProvider
  3. new Bot(config, services, logger)
  4. await bot.Run(CancellationToken.None)
  5. await Task.Delay(-1)        // keep process alive forever after Run() returns
  6. finally: Log.CloseAndFlush()
```

This is **not** the generic `Host.CreateDefaultBuilder()` pattern: `Program` builds a bare `ServiceCollection` and calls `BuildServiceProvider()` itself. `Bot` derives from `BackgroundService` but is never added to an `IHost` here — it's `new`'d directly and `Run()` is called by hand, so its `ExecuteAsync` override (which forwards to `Run`) is never invoked by a hosting layer in this codepath (contrast `DiscordBot.Dashboard/Program.cs`, which *does* register `Bot` via `AddHostedService<DiscordBot.Bot>()` on a real `WebApplication` host — see §1). `Program.cs` itself flags the manual DI setup with a comment `// No using statement?` next to `ConfigureServices(config)` — the returned `IServiceProvider` is never disposed.

### DI registration (`ConfigureServices` in `Program.cs`)

```csharp
serviceCollection
    .AddDiscordBot<Program>(config)     // DiscordBot/Configuration/ConfigurationExtensions.cs
    .UseLiteDbRepositories(config)      // DiscordBot.Data/Configuration/ConfigurationExtensions.cs
    .AddWiseOldManApi()                 // WiseOldManConnector/Configuration
    .AddDiscordBotServices()            // DiscordBot.Services/Configuration/ServiceConfigurationExtensions.cs
    .ConfigureQuartz(config);           // DiscordBot.Services/Configuration/QuartzConfiguration.cs
```

`AddDiscordBot<T>(config)` (`DiscordBot/Configuration/ConfigurationExtensions.cs`) wraps `AddDiscordBot(config, typeof(T))`, which fans out to private extension methods in the same file:

| Method | Registers |
|---|---|
| `AddLoggingInformation()` | `Log.Logger` singleton, `ILogService -> SerilogService`, `AddLogging(...AddSerilog(dispose:true))`, `IWiseOldManLogger -> WisOldManLogger` (transient) |
| `AddDiscordClient()` | `DiscordSocketClient` (singleton, from a `DiscordSocketConfig`), `CommandService`, `InteractiveCommandHandlerService`, `InteractiveService` (Fergun) — all singleton — plus `AddDiscordCommands()` |
| `AddDiscordCommands()` | `ICommandAuthorizationService -> CommandAuthorizationService` (singleton); each `IApplicationCommandHandler` as singleton (`PingApplicationCommandHandler`, `ManageCommandsApplicationCommandHandler`, `KillBotCommandHandler`, `CountConfigurationApplicationCommandHandler`, `ConfigureApplicationCommandHandler`, `CreateCompetitionCommandHandler`, `AuthorizationConfigurationCommandHandler`); `ICommandStrategy` (singleton, built from an explicit array of those handlers) |
| `AddExternalServices()` | `IDiscordService -> DiscordService` (transient); MediatR scan of `Program`'s assembly (`DiscordBot`) |
| `AddHelpers()` | `MetricTypeParser` (transient) |
| `AddConfiguration(configuration)` | Options binding — see §3 |
| `ConfigureAutoMapper()` | `AutoMapper.Mapper` singleton |
| `AddCommandsFromAssemblies(assemblies)` | `ICommandDefinitionProvider`, `ICommandInstigator` (singleton, `new`'d manually); `ICommandRegistrationService -> CommandRegistrationService` (transient), **decorated** via Scrutor `.Decorate<ICommandRegistrationService>(...)` with `CommandDefinitionRegistrationService` — the resolved type is always the "new" one, wrapping the old as `_oldRegistration` |

There is also an `[Obsolete]` `AddDiscordBot(IServiceCollection, IConfiguration)` overload that skips `AddCommandsFromAssemblies` entirely — kept for compatibility, not used by `Program.cs`.

`DiscordBot.Services/Configuration/ServiceConfigurationExtensions.cs`'s `AddDiscordBotServices()` adds transient business services (`IGroupService`, `IPlayerService`, `IOsrsHighscoreService`, `ICounterService`, `IAutomatedDropperService`, `IAuthorizationService`, `IGraveyardService`, `IClanFundsService`, `IConfirmationService`), a singleton `ICollectionLogItemProvider`, a Refit client `IOsrsWikiApi` (base address `https://oldschool.runescape.wiki/`), and a **second** `AddMediatR` scan of `DiscordBot.Services`'s own assembly (handlers thus live in two separately-scanned assemblies).

`DiscordBot.Data/Configuration/ConfigurationExtensions.cs`'s `UseLiteDbRepositories(config)` registers `MigrationManager` and `LiteDbManager` as singletons, 12 repository *factories* as transient (e.g. `GuildConfigLiteDbRepositoryFactory`, `PlayerLiteDbRepositoryFactory`, `CommandInfoRepositoryFactory`, `RunescapeDropperGuildConfigurationRepositoryFactory`), a singleton `IRepositoryStrategy` built from an explicit array of those factories, and binds `LiteDbOptions`.

### Hosted services / background work

- `Bot` (`DiscordBot/Bot.cs`) extends `BackgroundService` but is invoked manually via `bot.Run(...)`, not `IHost.RunAsync()`. `Run()`: starts `ConfigureDiscord()` and `ConfigureScheduler()` concurrently, then awaits both. `ConfigureDiscord()` resolves `DiscordSocketClient`, calls `InteractiveCommandHandlerService.SetupAsync()`, reads `BotConfiguration` directly from config (`_config.GetSection("Bot").Get<BotConfiguration>()` — a second, separate bind from the `IOptions` one in DI), then `client.LoginAsync(TokenType.Bot, botConfig.Token)` and `client.StartAsync()`. `ConfigureScheduler()` resolves `ISchedulerFactory` (Quartz) and calls `Start()`. Finally logs "Discord bot is running." and `await Task.Delay(-1, stoppingToken)`.
- Quartz jobs (`ConfigureQuartz`/`ConfigureJobs` in `QuartzConfiguration.cs`) are scheduled at startup: in-memory store (`UseInMemoryStore()`), Microsoft DI job factory, 10-thread pool. Jobs: `AutoUpdateGroupJob` (twice daily, 22:00 and 01:00 Europe/Berlin if available else local TZ), `TopLeaderBoardJob` (monthly, day 1 00:30), `MonthlyTopDeltasJob` (monthly, day 1 00:05). Code comment warns "this WON'T work with scoped services like EF Core's DbContext" — a caution against scoped DI lifetimes for jobs.

### Service lifetimes — key types

| Type | Lifetime | Registered in |
|---|---|---|
| `DiscordSocketClient`, `CommandService`, `InteractiveCommandHandlerService`, `InteractiveService` (Fergun) | Singleton | `ConfigurationExtensions.AddDiscordClient()` |
| `ICommandAuthorizationService`/`CommandAuthorizationService`, all `I*ApplicationCommandHandler` handlers, `ICommandStrategy`/`CommandStrategy` | Singleton | `AddDiscordCommands()` |
| `ILogService`/`SerilogService` | Singleton | `AddLoggingInformation()` |
| `IWiseOldManLogger`/`WisOldManLogger` | Transient | `AddLoggingInformation()` |
| `IDiscordService`/`DiscordService` | Transient | `AddExternalServices()` |
| `AutoMapper.Mapper` | Singleton | `AutoMapperConfiguration.ConfigureAutoMapper()` |
| `ICommandDefinitionProvider`, `ICommandInstigator` | Singleton | `AddCommandsFromAssemblies()` |
| `ICommandRegistrationService` (resolves to `CommandDefinitionRegistrationService`, wrapping `CommandRegistrationService`) | Transient | `AddCommandsFromAssemblies()` (Scrutor `.Decorate`) |
| `IGroupService`, `IPlayerService`, `IOsrsHighscoreService`, `ICounterService`, `IAutomatedDropperService`, `IAuthorizationService`, `IGraveyardService`, `IClanFundsService`, `IConfirmationService` | Transient | `ServiceConfigurationExtensions.AddServices()` |
| `ICollectionLogItemProvider` | Singleton | same |
| `IOsrsWikiApi` (Refit) | Refit default (per-request-ish) | `ServiceConfigurationExtensions.AddExternalServices()` |
| `LiteDbManager`, `MigrationManager`, `IRepositoryStrategy` | Singleton | `DiscordBot.Data...ConfigurationExtensions.UseLiteDbRepositories()` |
| All `*LiteDbRepositoryFactory` types, `IApplicationCommandInfoRepository` (via `CommandInfoRepositoryFactory.Create()`) | Transient | same |

## 3. Configuration

Config is loaded once, at startup, via `Program.BuildConfig()`:

```csharp
new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environmentName.ToLowerInvariant()}.json", optional: true)
    .Build();
```

`environmentName` comes from `Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")` — **if unset, `environmentName` is `null` and `.ToLowerInvariant()` throws a `NullReferenceException` before the app starts**, with no fallback. This is the only environment variable the host reads directly; despite `Microsoft.Extensions.Configuration.EnvironmentVariables` being referenced in the csproj, it is **not** wired into this `ConfigurationBuilder`, so env-var overrides of JSON keys are not currently active.

No `appsettings.json` exists in the repo (`.gitignore` excludes `appsettings.json`, `appsettings.*.json`, `/appsettings.debug.json`, `/appsettings.Development.json`). What *is* committed is `DiscordBot/appsettings.default.json`, a template. Its top-level keys: `Bot` (see below); `MetricSynonyms.Synonyms` — a big `Dictionary<MetricType, List<string>>` of skill/boss synonyms (no secrets); `Quartz` — `quartz.scheduler.instanceName`, `quartz.threadPool.maxConcurrency` (bound into `QuartzOptions`); `LiteDbOptions` — `PathPrefix`, `FileSuffix` (no secrets).

`Bot` section keys (bound to `DiscordBot.Configuration.BotConfiguration`):

```csharp
public class BotConfiguration {
    public string Token { get; set; }                      // Discord bot token, SECRET (sample value redacted to "Token")
    public MessageConfiguration Messages { get; set; }      // Messages.WaitMessages: string[]
    public string CustomPrefix { get; set; }                // e.g. "!bot"
    public BotTeamConfiguration TeamConfiguration { get; set; }
}
```

### `BotTeamConfiguration` — the "owner"/"team" concept

`DiscordBot/Configuration/BotTeamConfiguration.cs`:

```csharp
public class BotTeamConfiguration {
    public DiscordGuildId GuildId { get; set; }
    public DiscordUserId OwnerId { get; set; }
}
```

Bound from `Bot:TeamConfiguration` (`configuration.GetSection("Bot").GetSection(nameof(BotConfiguration.TeamConfiguration))` in `ConfigurationExtensions.AddConfiguration`). Sample values in `appsettings.default.json` are both `0`.

This **is** the home/owner-guild concept in this codebase — there is no separately named "admin guild" or "home guild" class; `BotTeamConfiguration.GuildId` fills that role:

- **`InteractiveCommandHandlerService`** exposes `private DiscordGuildId OwnerGuildId => _botTeamConfiguration.Value.GuildId;` and on startup (`InitializeCommands()`) registers `ManageCommandsApplicationCommandHandler` and `KillBotCommandHandler` **only as guild commands in that one guild** (`RegisterCommandForOwnersGuild` → `_client.Rest.CreateGuildCommand(..., OwnerGuildId.UlongValue)`) — never globally or in any other guild.
- **`CommandAuthorizationService`** reads `OwnerId` and, in `CheckAuthorization`, grants unconditional access to any command if `context.User.GetUserId() == OwnerId` (bot-owner bypass). It also reads `GuildId`, but the branch that would use it (a "user is in the bot's home guild" special case) is **commented out** — dead code today; `GuildId`'s only live effect is the admin-command registration above.
- Authorization otherwise falls back to: guild owner (`context.Guild.OwnerId`), then per-guild `CommandRoleConfig` (`UserIds`/`RoleIds` maps via `IGroupService.GetCommandRoleConfig`, cached in-memory per guild for 3 hours in a plain `Dictionary` field on the singleton service — not distributed, resets on restart).

Other config classes: `DiscordBot.Common.Configuration.MessageConfiguration` (`WaitMessages: List<string>`); `MetricSynonymsConfiguration` (`Synonyms: Dictionary<MetricType, List<string>>`, bound from `MetricSynonyms`); `WiseOldManConfiguration` (`GroupId`, `GroupVerificationCode`, `GroupName`); `WiseOldManConnector.Configuration.WiseOldManOptions` (bound from `WiseOldMan` section); `DiscordBot.Data.Configuration.LiteDbOptions` (`PathPrefix`, `FileSuffix`, `SectionName = "LiteDbOptions"`, bound from `LiteDbOptions` section in `UseLiteDbRepositories`).

`BotConfiguration` itself is bound/constructed **four separate times** from the same `Bot` section: `IOptions<BotConfiguration>`, a raw singleton instance (`.AddSingleton(botConfiguration)` / `.AddSingleton(botConfiguration.Messages)`), a third direct `configuration.GetSection("Bot").Get<BotConfiguration>()` in `AddConfiguration` used to build that singleton, and a fourth ad hoc read in `Bot.ConfigureDiscord()` to fetch the token. No single source-of-truth object flows through.

## 4. Discord client

- **Discord.Net version**: `3.10.0` (`DiscordBot.csproj`). Fergun.Interactive `1.7.1`.
- **Client construction** (`ConfigurationExtensions.AddDiscordClient`):
  ```csharp
  new DiscordSocketConfig {
      AlwaysDownloadUsers = true,
      MessageCacheSize = 100,
      GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.GuildMessages |
                       GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers |
                       GatewayIntents.Guilds
  }
  ```
  `GuildMembers` is listed twice (harmless copy/paste artifact). `MessageContent` intent is **not** requested — verify before adding any feature that needs to read arbitrary message text outside slash-command interactions.
- **Login/start**: `Bot.ConfigureDiscord()` calls `client.LoginAsync(TokenType.Bot, botConfig.Token)` then `client.StartAsync()`. No retry/backoff around login.
- **Events subscribed** — a small, specific set; there is **no** `Ready`, `GuildAvailable`, `JoinedGuild`, or `LeftGuild` handler anywhere in the codebase:
  - `DiscordSocketClient.Log` → `SerilogService.LogDiscordClient`, and `CommandService.Log` → `SerilogService.LogCommand` (both wired in `SerilogService`'s constructor).
  - `DiscordSocketClient.InteractionCreated` → `InteractiveCommandHandlerService.OnInteraction` (wired in its ctor) — the single entry point for slash-command/component/autocomplete interactions; dispatches to `ApplicationCommandContext`/`MessageComponentContext`/`AutocompleteCommandContext` by `SocketInteraction` subtype, calls `ICommandInstigator.ExecuteCommandAsync`, falls back to `ICommandStrategy.HandleInteractiveCommand` on a "404"-tagged failure.
  - `DiscordSocketClient.Connected` → `InteractiveCommandHandlerService.ClientOnConnected` (only if `SetupAsync()` runs before the client is already connected; unsubscribes itself on first fire, then `Initialize()` → `InitializeCommands()`: registers the owner-guild-only admin commands, then `ICommandRegistrationService.UpdateAllCommands(...)` for everything else).
- **How the bot knows which guilds it's in**: there is **no persisted registry of "registered guilds"** for this purpose. Every place needing "the guilds the bot is currently in" — `DiscordService.GetGuilds()`, `CommandDefinitionRegistrationService.HandleGuildRegistries`, `CommandRegistrationService.HandleGuildRegistries` — reads `_client.Guilds` live from `DiscordSocketClient`'s gateway-populated cache, returning a failure if `_client.ConnectionState != ConnectionState.Connected`. (There *is* a persisted, per-command `RegisteredGuilds` list on `ApplicationCommandInfo` in LiteDB, but that means "which guilds should have command X registered," not "which guilds is the bot a member of" — it's cross-referenced against `_client.Guilds` at registration time and guilds outside the live list are skipped regardless of what `RegisteredGuilds` says.)
- Slash-command registration happens in two parallel places doing near-identical guild/global diffing: `CommandRegistrationService` (the "old" implementation) and `CommandDefinitionRegistrationService` (the "new" one, added via Scrutor decoration, falling back to the old one for any command not found in `ICommandDefinitionProvider`'s definitions). Both compare Discord's current global/guild commands against the stored `ApplicationCommandInfo.Hash` and delete+recreate on mismatch.

## 5. Logging

Two independent Serilog pipelines exist:

- **Global logger**, set up in `Program.ConfigureSerilogger()` (`DiscordBot/Program.cs`): `LoggerConfiguration().Enrich.FromLogContext().MinimumLevel.Debug().WriteTo.File(new JsonFormatter(), "logs/osrs_bot.log", rollingInterval: RollingInterval.Day).WriteTo.Console(LogEventLevel.Information)`. This is the only sink config the console host uses — **hardcoded in C#, not driven by `appsettings.json`** (no `Serilog` section is consumed). File sink: JSON lines to `logs/osrs_bot.log` (relative to CWD, daily rolling); console sink at `Information`+.
- `AddLoggingInformation()` also does `AddSingleton(_ => Log.Logger)` and `AddLogging(b => b.AddSerilog(dispose: true))`, so both raw `Serilog.ILogger` and `Microsoft.Extensions.Logging.ILogger<T>` resolve through the same static `Log.Logger`.
- `ILogService`/`SerilogService` (`DiscordBot/Services/SerilogService.cs`) forwards Discord.Net `LogMessage`s (severity-mapped) and ad hoc `Log(...)` calls to the static `Serilog.Log` façade directly — it does not use the injected `ILogger<T>` pattern.
- `DiscordBot.Dashboard/Program.cs` has its own separate Serilog bootstrap (`logs/web.log`, `builder.WebHost.UseSerilog()`) — a second, independent logging pipeline.
- No health checks, metrics, or APM/telemetry integration exists anywhere in scope.

## 6. Deployment

`DiscordBot/Dockerfile` (multi-stage):

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
  COPY [only the 4 .csproj files for DiscordBot, DiscordBot.Common, WiseOldManConnector, DiscordBot.Services]
  RUN dotnet restore "DiscordBot/DiscordBot.csproj"
  COPY . .
  RUN dotnet build "DiscordBot.csproj" -c Release -o /app/build
FROM build AS publish
  RUN dotnet publish "DiscordBot.csproj" -c Release -o /app/publish
FROM base AS final
  COPY --from=publish /app/publish .
  ENTRYPOINT ["dotnet", "DiscordBot.dll"]
```

Notes/gotchas:

- The restore layer only copies `.csproj` for `DiscordBot`, `DiscordBot.Common`, `WiseOldManConnector`, `DiscordBot.Services` — `DiscordBot.Data`'s csproj (referenced transitively via `DiscordBot.Services`) isn't copied ahead of time, so the restore-cache layer trick is incomplete for it (not a correctness bug, just a weaker cache).
- **No `VOLUME` declaration.** LiteDB files are written relative to CWD (`{PathPrefix}{guildId or "common"}_{FileSuffix}.db`, from `LiteDbManager`), which in the container is `/app` (the `WORKDIR`). A deployer must bind-mount/persist `/app` themselves or all data is lost on container recreation. Same for relative `logs/osrs_bot.log`.
- No `ENV`/`ARG` plumbing for the token in the Dockerfile, and (§3) env-var config isn't wired into `BuildConfig()` anyway. A deployer must supply an `appsettings.json` file inside `/app` (start from `appsettings.default.json`) and set `ASPNETCORE_ENVIRONMENT` or the process throws at startup.
- `.dockerignore` excludes `appsettings.development.json`/`appsettings.production.json` from the build context entirely, so only a real `appsettings.json` (typically absent — gitignored) or `appsettings.default.json` ever reaches the image via the csproj's `CopyToOutputDirectory` rule.
- `justfile` at repo root is currently an **empty file** (0 bytes, added in commit `4edacce`) — no recipes yet.

## 7. Building, running, and testing locally

`dotnet --version` → **`7.0.410`**, already on `PATH` here via the Nix devShell (`flake.nix` pins `dotnetCorePackages.sdk_7_0`; `.envrc` is `use flake .`) — no explicit `nix develop -c` prefix was needed.

`dotnet build OrsrDiscordAutomator.sln` → **0 errors, 125 warnings**; the whole solution compiles as-is. Warnings are almost all `DiscordBot.Dashboard` nullable-reference warnings (`CS8618`/`CS8600`/`CS8602`/`CS8619`) from Razor components with non-nullable `[Parameter]`/injected properties — none block the build.

Running locally (console host): from `DiscordBot/`, with a real `appsettings.json` present (copy `appsettings.default.json`, fill in `Bot.Token`) and `ASPNETCORE_ENVIRONMENT` set to any value:
```
ASPNETCORE_ENVIRONMENT=Development dotnet run --project DiscordBot/DiscordBot.csproj
```
`.vscode/launch.json` is **stale/unrelated** — its only real `coreclr` config targets `CepAccounts.Tests` (not present in this repo). The Rider `.run/*.xml` configs ("Without web" → `DiscordBot`, "With web" → `DiscordBot.Dashboard`, "WOMReader") are more relevant but hardcode `net6.0` as the TFM despite every csproj targeting `net7.0` — these predate the .NET 7 upgrade (`3268ce6`/`2ba32ad`) and haven't been refreshed.

Tests: `dotnet test tests/DiscordBot.ServicesTests/DiscordBot.ServicesTests.csproj` (xUnit + FluentAssertions + NSubstitute + AutoBogus) → **57 passed, 5 failed, 3 skipped (65 total)** on this checkout. Failures include `MediaWikiToItemParserTests.ParsingShouldHaveAllItems` (expects exactly 1326 items, actually gets 1404 — a stale fixture-count assertion) and an `AutomatedDropperServiceTests` failure inside NSubstitute's call routing. **Treat these 5 as pre-existing known-red tests**; re-run before/after touching the wiki item parser or `AutomatedDropperService` scheduling to compare. `tests/WebAppTests` project-references `DiscordBot.Dashboard` and exercises embed/JSON deserialization fixtures; `WiseOldManConnectorTests` covers the WOM client.

## 8. Gotchas for an AI editing this code

- `BuildConfig()` throws `NullReferenceException` on `environmentName.ToLowerInvariant()` if `ASPNETCORE_ENVIRONMENT` is unset — no fallback. Always set it (any value) locally and in Docker.
- `BotConfiguration` is bound four separate times from the same `Bot` section (§3) — if you add a field, verify all four paths see it; there's no single "current config" object.
- `BotTeamConfiguration.GuildId` ("owner guild") is live (admin slash commands register only there via `InteractiveCommandHandlerService.RegisterCommandForOwnersGuild`), but the same `GuildId` read inside `CommandAuthorizationService` is **dead code** (its usage branch is commented out). Only `OwnerId` (single Discord user ID) grants a real authorization bypass today.
- There is **no persisted "which guilds is the bot in" table**. Anything needing that must read `DiscordSocketClient.Guilds` live and handle `ConnectionState != Connected` (see `DiscordService.GetGuilds()`).
- Two nearly-identical command-registration implementations exist side by side (`CommandRegistrationService` and `CommandDefinitionRegistrationService`, composed via Scrutor `.Decorate<ICommandRegistrationService>`). Changing guild/global command diffing logic likely means changing **both**, depending on whether a command routes through `ICommandDefinitionProvider` (new path) or the old `ICommandStrategy` handler list (falls back to old path).
- Two independent MediatR assembly scans exist (`DiscordBot`'s own assembly, and `DiscordBot.Services`'s). A handler in the wrong assembly relative to what you expect scanned will silently not be found.
- `DiscordBot.Dashboard` also hosts `DiscordBot.Bot` as a hosted service on its own ASP.NET Core host (port 5829). Running it alongside the console `DiscordBot` against the same token/LiteDB files would double-start the bot. Only the console project is built into the Docker image; confirm which host is meant before changing "the bot."
- `DataLayer`, `ServiceLayer` (empty `Class1.cs` scaffolds) and `src/DiscordBot.Components` (template Razor scaffold) hold no real functionality — grep before assuming code lives there.
- LiteDB/log files are written relative to **current working directory**, not a fixed path — `/app` in Docker, with no volume declared (§6). Different launch CWDs produce different `db_*.db`/`logs/*.log` locations.
- `CommandAuthorizationService`'s per-guild `CommandRoleConfig` cache is a plain in-memory dictionary with a 3-hour TTL, not distributed/invalidated on write — role config changes can take up to 3h to take effect without a restart.
- `GatewayIntents` in `AddDiscordClient()` lists `GuildMembers` twice (harmless) and omits `MessageContent` — reading raw message text outside interactions likely needs that intent added (and enabled in the Discord Developer Portal).
- The 5 currently-failing tests in `DiscordBot.ServicesTests` (§7) are pre-existing; re-run `dotnet test` before/after a change to confirm you haven't added new failures, don't "fix" them incidentally.
- `README.md` is unedited GitHub-template boilerplate (still says ".NET 5") — trust the `.csproj` (`net7.0`) and `flake.nix` instead.
