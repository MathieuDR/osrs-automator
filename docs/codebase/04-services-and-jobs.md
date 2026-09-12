# 04 - Services and Jobs

Scope: `DiscordBot.Services` (business services, Quartz jobs, external HTTP), the
domain models in `DiscordBot.Common`, `DiscordBot/Services/DiscordService.cs` (the
Discord.Net abstraction other layers call into), the `job` slash command
(`DiscordBot/Commands/Interactive2/Job/**`), and `tests/DiscordBot.ServicesTests`.

All paths below are relative to the repo root
`/home/thieu/development/sources/osrs-automator`.

## 1. Service inventory

All services below live in `DiscordBot.Services/Services/*.cs` unless noted, and
are registered in `DiscordBot.Services/Configuration/ServiceConfigurationExtensions.cs`
(`AddServices()`) or `DiscordBot/Configuration/ConfigurationExtensions.cs`
(`AddExternalServices()` for `IDiscordService`). Most concrete classes are
`internal` — only reachable through their interface, which lives in
`DiscordBot.Services/Interfaces/*.cs`.

| Interface | Implementation | Responsibility | DI lifetime | Main dependencies |
|---|---|---|---|---|
| `ICollectionLogItemProvider` | `CollectionLogItemProvider` | Fetches & caches (in-memory, process-lifetime) the list of OSRS collection log item names from the wiki | Singleton | `IOsrsWikiApi` |
| `IPlayerService` | `PlayerService` | Couples a Discord user to one or more OSRS/WOM accounts, sets default account, enforces nicknames | Transient | `IRepositoryStrategy`, `IDiscordService`, `IOsrsHighscoreService` |
| `IGroupService` | `GroupService` | Per-guild WOM group config, timezone, auto-add, per-job channel config, leaderboard/competition creation, ad-hoc job queueing | Transient | `IRepositoryStrategy`, `IOsrsHighscoreService`, WOM `IWiseOldManCompetitionApi`/`IWiseOldManGroupApi`, `ISchedulerFactory` |
| `IOsrsHighscoreService` | `WiseOldManConnectorService` | Thin wrapper over the WOM connector's Player/Group/Competition/Name REST APIs | Transient | WOM `IWiseOldMan*Api` clients |
| `ICounterService` | `CountService` | "Counting" points system: award/remove points, thresholds → role grants, self-count request flow | Transient | `IRepositoryStrategy`, `IDiscordService`, `IConfirmationService` |
| `IAutomatedDropperService` | `AutomatedDropperService` | Per-user webhook endpoints for an external drop-tracking client; batches incoming drops via a debounced Quartz job | Transient | `IRepositoryStrategy`, `ISchedulerFactory` |
| `IAuthorizationService` | `AuthorizationService` | Grants/revokes `AuthorizationRoles` bit-flags to Discord users/roles for command gating | Transient | `IRepositoryStrategy` |
| `IGraveyardService` | `GraveyardService` | "Shame"/Graveyard feature — opt-in death log per user | Transient | `IRepositoryStrategy` |
| `IClanFundsService` | `ClanFundsService` | Tracks clan fund deposits/withdrawals/donations and a live donation leaderboard message | Transient | `IRepositoryStrategy`, `IDiscordService` |
| `IConfirmationService` | `ConfirmationService` | Generic "post an embed with Accept/Decline buttons, execute a `MediatR` command on accept" workflow | Transient | `IRepositoryStrategy`, `IDiscordService`, `IMediator` |
| `IJobService` | *(interface only — no implementation found in this scope; not registered in `AddServices()`)* | Intended to expose `ChannelJobConfiguration` lookup by `JobType` | — | — |
| `IDiscordService` | `DiscordService` (`DiscordBot/Services/DiscordService.cs`, **not** in `.Services` project) | Every outbound call into Discord.Net — sending messages/embeds, role management, guild/channel/user lookups | Transient | `DiscordSocketClient` (Singleton) |

Base classes used by most services (`DiscordBot.Services/Services/`):
- `BaseService` — holds `ILogger Logger`.
- `RepositoryService : BaseService` — adds `IRepositoryStrategy RepositoryStrategy` and a
  `GetRepository<T>(DiscordGuildId? guildId = null)` helper that resolves a LiteDB
  repository, per-guild if an id is given.
- `BaseGuildConfigurationService : RepositoryService` — adds `GetGuildConfig`/`SaveGuildConfig`
  helpers around `IGuildConfigRepository`. Used by `AuthorizationService`.

Note: `IJobService` (`DiscordBot.Services/Interfaces/IJobService.cs`) declares
`GetConfigurationForJobType(JobType)` but has no implementing class anywhere in
scope, and is absent from `AddServices()`. Treat it as dead/unfinished code — do
not assume it is wired up.

## 2. `IDiscordService` in detail

File: `DiscordBot.Services/Interfaces/IDiscordService.cs` (interface, in the
`.Services` project) — implemented by `DiscordBot/Services/DiscordService.cs`
(in the main bot project, because it needs `DiscordSocketClient`). This is
**the only sanctioned way** for services/jobs to talk to Discord.Net; nothing
in `DiscordBot.Services` references `Discord.WebSocket` directly except through
this interface.

Registered as `Transient` (`services.AddTransient<IDiscordService, DiscordService>()`
in `DiscordBot/Configuration/ConfigurationExtensions.cs`), but the underlying
`DiscordSocketClient` is a `Singleton`, so every instance talks to the same
live gateway connection.

Every method, in declaration order:

| Method | What it does in Discord.Net terms |
|---|---|
| `Task<Result> SetUsername(GuildUser user, string nickname)` | `_client.GetGuild(id).GetUser(id)` then `IGuildUser.ModifyAsync(x => x.Nickname = nickname)`. Fails with `Result.Fail` if guild or user isn't found in the socket cache. |
| `Task<Result> PrintRunescapeDataDrop(RunescapeDropData data, DiscordGuildId guildId, DiscordChannelId channelId)` | Resolves the guild's `SocketTextChannel`, sends a summary text message, a second message listing individual drops, then one `SendFileAsync` per attached image (each image is a base64 string converted to a `MemoryStream`). |
| `Task<Result<IEnumerable<Guild>>> GetGuilds()` | Returns `_client.Guilds` (mapped to `Guild` DTOs via `ToGuildDto()`). Fails if `_client.ConnectionState != ConnectionState.Connected`. **This is the call to enumerate every guild the bot is in** — used by `BaseGuildJob` (see §4). |
| `Task<Result<IEnumerable<Channel>>> GetChannelsForGuild(DiscordGuildId guildId)` | `_client.GetGuild(id).Channels`, mapped to `Channel` DTOs. |
| `Task<Result<Dictionary<Channel, IEnumerable<Channel>>>> GetNestedChannelsForGuild(DiscordGuildId guildId)` | Same as above, then groups channels under their parent category via `ChannelDtoExtensions.NestChannels()`. |
| `Task<Result> SendFailedEmbed(DiscordChannelId channelId, string message, Guid traceId)` | Builds a red-ish default `EmbedBuilder` with title "Failed to update group.", the message as description, and a `TraceId` field; sends via the private `SendEmbed` helper. |
| `Task<Result> SendSuccessEmbed(DiscordChannelId channelId, string message)` | `EmbedBuilder` decorated with `.WithSuccess(message)` (green, title "Success!") + `.AddCommonProperties()` (timestamp + dark-purple color), sent via `SendEmbed`. **This is the simplest "post one message to one channel" primitive** — used by `CountService.SendMessage`. |
| `Task<Result> SendWomGroupSuccessEmbed(DiscordChannelId channelId, string message, int groupId, string groupName)` | Same as success embed but with a Wise Old Man author block (`AddWiseOldMan`) linking to `https://wiseoldman.net/groups/{groupId}`. Used by `AutoUpdateGroupJob`. |
| `Task<Result> MessageLeaderboards<T>(DiscordChannelId channelId, IEnumerable<MetricTypeLeaderboard<T>> leaderboards)` | Builds one code-block-formatted plain-text message per leaderboard/metric, packs them into ≤1990-char chunks (`CreateCompoundedMessagesForMultipleMessages`), then sends each chunk as a plain `SendMessageAsync` on the resolved `ISocketMessageChannel`. Used by `TopLeaderBoardJob`/`MonthlyTopDeltasJob`. |
| `Task<Result> TrackClanFundEvent(DiscordGuildId guildId, ClanFundEvent clanFundEvent, DiscordChannelId clanFundsChannelId, long clanFundsTotalFunds)` | Builds an embed describing a fund event (type/amount/who/reason/running total) and posts it (fire-and-forget `_ = channel.SendMessageAsync(embed:...)`) to the guild's fund-tracking channel. |
| `Task<Result<DiscordMessageId>> UpdateDonationMessage(...)` | Builds a `DiscordLeaderBoard<string>` of top donors, posts it as a new message, deletes the previous leaderboard message (`DeleteMessage` helper via `GetMessageAsync`+`DeleteAsync`), returns the new message id so the caller can persist it for the next update. |
| `Task<Result<IEnumerable<GuildUser>>> GetUsers(DiscordGuildId guildId)` | `_client.GetGuild(id).Users` mapped to `GuildUser` DTOs. |
| `Task<Result> AddRoles(DiscordGuildId guild, Dictionary<DiscordUserId, IEnumerable<DiscordRoleId>> userDicts)` | For each user, `IGuildUser.AddRolesAsync(roleIds)`. |
| `Task<Result> RemoveRoles(...)` | Same, `RemoveRolesAsync`. |
| `Task<Result<DiscordMessageId>> SendConfirmationMessage(DiscordChannelId channelId, string title, string description, EmbedFieldDto[] fields, string thumbnailUrl = null)` | Builds an embed with title/description/fields/thumbnail plus an `Accept`/`Decline` `ComponentBuilder` (custom ids `confirm:confirmed` / `confirm:declined`), sends it, returns the resulting message id — consumed by `IConfirmationService.CreateConfirm`. |

Private helper `SendEmbed(DiscordChannelId, EmbedBuilder, ComponentBuilder?)`
does the actual `_client.GetChannelAsync(id)` → cast to `ISocketMessageChannel`
→ `SendMessageAsync(...)`, wrapped in a `Result`. This is the common choke
point all embed-sending public methods funnel through.

**Recipe: "post a message to a channel in every registered guild".** There is
no existing single method for this, but the composition is:
1. `var guildsResult = await _discordService.GetGuilds();` — enumerates every
   guild the bot's socket client currently sees (same call `BaseGuildJob` uses).
2. For each `Guild`, resolve whatever channel you want (either a fixed/derived
   channel id, or read it from that guild's persisted config — see §4), and
   call an existing send method such as `SendSuccessEmbed(channelId, message)`
   or `MessageLeaderboards(...)`. There's no raw "SendPlainMessage" method
   on the interface aside from those two purpose-built ones — a new agent
   adding a generic broadcast would either reuse `SendSuccessEmbed` or add a
   new method to `IDiscordService`/`DiscordService` following the same
   `_client.GetGuild(id).GetTextChannel(id).SendMessageAsync(...)` pattern seen
   throughout the class.

## 3. Domain features

### Counting / SelfCount
`ICounterService`/`CountService` (`DiscordBot.Services/Services/CountService.cs`).
Tracks a per-guild point total per user (`UserCountInfo`, one LiteDB doc per
user via `IUserCountInfoRepository`). `Count(...)` appends a `Count` history
entry, updates totals, evaluates `CountThreshold`s (`GuildConfig.CountConfig.Thresholds`)
to auto-grant/revoke Discord roles via `IDiscordService.AddRoles`/`RemoveRoles`,
and posts a summary via `IDiscordService.SendSuccessEmbed` to the guild's
configured `CountConfig.OutputChannelId`. "SelfCount" is the user-initiated
variant: a user requests points for an item (`SelfCount(...)`), which builds a
`SelfCountConfirmCommand` (`DiscordBot.Common/Models/Commands/SelfCountConfirmCommand.cs`)
and routes it through `IConfirmationService.CreateConfirm` rather than
awarding points directly — points are only granted once an admin clicks
Accept, which invokes `SelfCountHandler` (`DiscordBot.Services/CommandHandlers/SelfCountHandler.cs`,
a MediatR `IRequestHandler<SelfCountConfirmCommand, Result>`) which calls
`ICounterService.Count(...)` (and again for `Splits` if the item is splittable).
Item catalog and request-channel gating live in `SelfCountConfiguration`
(`ISelfCountConfigurationRepository`).

### Graveyard / Shame
`IGraveyardService`/`GraveyardService`. **Not related to messaging/automation** —
it is an in-clan "shame" log of player deaths. Users must `OptIn` before they
can be shamed or shame others (`Shame(...)` checks both the shamer's and
shamed's opt-in status). A `Shame` record captures a `ShameLocation` enum, an
optional WOM `MetricType`, an image URL, and who reported it, stored per-user
inside one `Graveyard` document per guild (`IGraveyardRepository`,
`AddShame`/`GetShameById`/`UpdateShame`/`RemoveShame`). Timestamps are
converted to the guild's configured timezone on read (`SetTimezone`, reusing
`DateTimeHelper.ToOffset`).

### ClanFunds
`IClanFundsService`/`ClanFundsService`. One `ClanFunds` document per guild
holds a running list of `ClanFundEvent`s (Deposit/Withdraw/Donation/Refund/
Other/System) and the channel ids for a tracking channel and a donation
leaderboard channel. `AddClanFund` validates the event, appends it, then calls
private `TrackEvent` which (a) posts the event via
`IDiscordService.TrackClanFundEvent`, and (b) if the event is a `Donation`,
recomputes the top-50 donor leaderboard and calls
`IDiscordService.UpdateDonationMessage` to edit-by-replace the pinned
leaderboard message, persisting the new message id back onto the `ClanFunds`
record. `Initialize` sets up the two channels and can backfill/reconcile the
total via a synthetic `System` event.

### Confirmation
`IConfirmationService`/`ConfirmationService` — a generic "ask an admin to
approve an action" primitive built on `IConfirmCommand` (a `MediatR`
`IRequest<Result>` with `Title`/`Description`/`Fields`/`ImageUrl`/`Handler`).
`CreateConfirm` posts the embed+buttons via
`IDiscordService.SendConfirmationMessage` and persists a `Confirmation` record
keyed by the resulting Discord message id. `Confirm(accepted, messageId,
confirmedBy)` looks the pending confirmation up, and if accepted, dispatches
the stored `IConfirmCommand` through `IMediator.Send` (this is how
`SelfCountConfirmCommand` reaches `SelfCountHandler`); it then marks the
`Confirmation` as confirmed/denied. The Discord-side button click is wired in
`DiscordBot/Commands/Interactive2/Confirm/ConfirmButtonHandler.cs` (custom id
`confirm:confirmed`/`confirm:declined` from `SendConfirmationMessage`).

### Drops / AutomatedDropper
`IAutomatedDropperService`/`AutomatedDropperService` — backs an external
screenshot/webhook client that POSTs OSRS drop data to a per-user generated
endpoint (`RequestUrl` mints an `EndpointId`, stored in
`DropperGuildConfiguration.UserEndpoints`). `HandleDropRequest` appends the
incoming drop/image to an "active" `RunescapeDropData` record
(`IRuneScapeDropDataRepository`) and (re)schedules a one-shot Quartz job
(`HandleRunescapeDropJob`) 10 seconds out (60s if the drop source name
contains "clue") via `ScheduleJob`/`RescheduleJob` — this batches rapid-fire
drop reports into a single Discord post. See §4 for the job itself. Per-guild
per-channel filtering (`DropperConfiguration`: value/rarity/white/blacklists)
exists as a data model but the actual filter logic in
`HandleRunescapeDropJob.FilterData` is commented out — currently **all**
non-empty drops are sent unfiltered except pets always pass through.

### Player / Group management (Wise Old Man)
`IPlayerService`/`PlayerService` couples a Discord user to one or more WOM
player accounts (`Player` model from `WiseOldManConnector`), enforces a
nickname template (`GuildUser` nickname set via `IDiscordService.SetUsername`
when `EnforceNameTemplate` is on), and can request WOM name changes.
`IGroupService`/`GroupService` owns the per-guild `GuildConfig` (WOM group id
+ verification code, timezone, auto-add-new-accounts, automated-job channel
map, command role config), fetches leaderboards/competitions from WOM
(delegating to `IOsrsHighscoreService`), and exposes `QueueJob(JobType)` /
`SetAutomationJobChannel(...)` used by the `job` slash command (§4).
`IOsrsHighscoreService`/`WiseOldManConnectorService` is a thin pass-through
over the `WiseOldManConnector` NuGet package's `IWiseOldManPlayerApi` /
`IWiseOldManGroupApi` / `IWiseOldManCompetitionApi` / `IWiseOldManNameApi`.

### Collection log items
`ICollectionLogItemProvider`/`CollectionLogItemProvider` — fetches the OSRS
Wiki "Collection log" page via `IOsrsWikiApi.GetPage`, parses the wikitext
with `MediaWikiContentToItemsParser.GetItems` (regex over `{{plink|...}}`
templates), and caches the resulting item-name list for the process lifetime
(Singleton) until `ResetCache()` is called. Intended to drive collection-log
based drop filtering in `AutomatedDropperService`/`HandleRunescapeDropJob`,
but that filter logic is currently commented out (see above).

## 4. Quartz jobs

### Scheduler setup
`DiscordBot.Services/Configuration/QuartzConfiguration.cs` —
`ConfigureQuartz(IServiceCollection, IConfiguration)`:
- Reads base `QuartzOptions` from the `"Quartz"` config section, then forces
  `IgnoreDuplicates = true` and `OverWriteExistingData = true`.
- `SchedulerId = "Scheduler-Core"`.
- `UseMicrosoftDependencyInjectionJobFactory()` — jobs are resolved through DI,
  so job constructors can take any registered service (this is why jobs are
  **not** required to have a public parameterless constructor).
- `UseDefaultThreadPool(tp => tp.MaxConcurrency = 10)`.
- `UseSimpleTypeLoader()` + `UseInMemoryStore()` — **no persistent job store**;
  all scheduled jobs/triggers are lost on process restart except the four
  statically declared ones below, which get re-registered from code on every
  boot.
- `ConfigureJobs()` statically schedules four triggers, all in the
  `Europe/Berlin` time zone if available (else machine-local):
  - `AutoUpdateGroupJob` × 2 triggers: `"GroupUpdate-evening"` daily at 22:00
    and `"GroupUpdate-morning"` daily at 01:00, group `"wom"`.
  - `TopLeaderBoardJob`: monthly, day 1 at 00:30, group `"wom"`.
  - `MonthlyTopDeltasJob`: monthly, day 1 at 00:05, group `"wom"`.
  - All four use `.WithMisfireHandlingInstructionFireAndProceed()`.

Additional jobs can be scheduled **dynamically** at runtime — see
`GroupService.QueueJob(JobType)` (ad-hoc, ~5s from now, no periodic re-trigger)
and `AutomatedDropperService.ScheduleJob` (debounced one-shot per user for
`HandleRunescapeDropJob`).

### Job class hierarchy
All in `DiscordBot.Services/Jobs/`:

- **`BaseJob : IJob`** — Quartz entry point. `Execute(IJobExecutionContext)`
  opens a logger scope (`jobType`, `jobKey`, `triggerKey`, `scheduledFor`,
  `refire`), stores `Context`, calls abstract `DoWork()`, and logs on
  `Result.IsFailed`. Every job ultimately returns a `FluentResults.Result`.
- **`RepositoryJob : BaseJob`** — adds `IRepositoryStrategy RepositoryStrategy`.
  Used directly by `HandleRunescapeDropJob` (not guild-scoped — it operates on
  one user's active drop record and fans out to whichever guilds have it
  configured).
- **`BaseGuildJob : BaseJob`** — adds `IDiscordService DiscordService`.
  `DoWork()` calls `DiscordService.GetGuilds()`, then runs abstract
  `DoWorkForGuild(Guild guild)` **concurrently for every guild** the bot is in
  (`Task.WhenAll`, wrapped in a per-guild logger scope), merging results with
  `FluentResults.Result.Merge`.
- **`ConfigurableGuildJob : BaseGuildJob`** — adds `JobType JobType` and
  `IRepositoryStrategy RepositoryStrategy`. Its `DoWorkForGuild` implementation
  looks up that guild's `GuildConfig` (via `IGuildConfigRepository`), then
  `guildConfiguration.AutomatedMessagesConfig.ChannelJobs.TryGetValue(JobType,
  out jobConfiguration)`. If no entry exists for this `JobType` in this guild,
  the guild is silently skipped (`Result.Ok()`), no work done, no message
  sent. Otherwise it calls abstract
  `DoWorkForGuildWithContext(Guild, GuildConfig, ChannelJobConfiguration)`
  — subclasses do **not** themselves check `IsEnabled`/resolve the channel,
  except `AutoUpdateGroupJob` which additionally short-circuits if
  `!configuration.IsEnabled`.

Concrete jobs:

| Job | Base | `JobType` | What it does |
|---|---|---|---|
| `AutoUpdateGroupJob` | `ConfigurableGuildJob` | `GroupUpdate` | Calls `IOsrsHighscoreService.UpdateGroup(womGroupId, verificationCode)`, then posts via `DiscordService.SendWomGroupSuccessEmbed(configuration.ChannelId, ...)` on success, or `DiscordService.SendFailedEmbed` on WOM `BadRequestException`. |
| `TopLeaderBoardJob` | `ConfigurableGuildJob` | `MonthlyTop` | For every queryable `MetricType`, fetches `IOsrsHighscoreService.GetLeaderboard(groupId, metric)`, then `DiscordService.MessageLeaderboards(configuration.ChannelId, tops)`. |
| `MonthlyTopDeltasJob` | `ConfigurableGuildJob` | `MonthlyTopGains` | Same shape as above but `GetTopDeltasOfGroup(groupId, metric, Period.Month)`. |
| `HandleRunescapeDropJob` | `RepositoryJob` | *(none — not guild-configurable)* | Reads the "active" `RunescapeDropData` for the endpoint's user (`Context.MergedJobDataMap.GetLongValue("endpoint")`), determines which guilds should see it (`GetGuildIdsForEndpoint` — currently **hardcoded** to a single guild id, `403539795944538122`), iterates that guild's `DropperGuildConfiguration.ChannelConfigurations`, and calls `_discordService.PrintRunescapeDataDrop(filteredData, guildId, channelId)` per channel. Deletes the active record once all configured guilds have been messaged. |
| `MemoryReportJob` | `BaseJob` | *(none — diagnostic only)* | Logs process memory metrics (working set, GC heap, OS handle count) and the count of open LiteDB databases every 30 minutes. Used to monitor resource usage and verify that the lease-based lifecycle is properly closing unused databases. |

### Recipe: how a job finds its channel and posts — "periodically post into a guild channel"
This is the closest existing pattern for "post a message into a guild channel
on a schedule", and the pattern to copy for a new one:

1. Add a `JobType` enum value (`DiscordBot.Common/Models/Enums/JobType.cs`).
2. Write a job class `: ConfigurableGuildJob`, constructor takes
   `ILogger<T>, IDiscordService, IRepositoryStrategy` (+ anything else needed),
   and passes the new `JobType` to the base constructor:
   ```csharp
   public class MyJob : ConfigurableGuildJob {
       public MyJob(ILogger<MyJob> logger, IDiscordService discordService, IRepositoryStrategy repositoryStrategy)
           : base(logger, discordService, JobType.MyNewJob, repositoryStrategy) { }

       protected override async Task<Result> DoWorkForGuildWithContext(
           Guild guild, GuildConfig guildConfig, ChannelJobConfiguration configuration) {
           if (!configuration.IsEnabled) return Result.Ok();
           // ... do work ...
           return await DiscordService.SendSuccessEmbed(configuration.ChannelId, "message");
       }
   }
   ```
   The base class has already: enumerated all guilds
   (`DiscordService.GetGuilds()`), loaded each guild's `GuildConfig`, and
   resolved `configuration.ChannelId` from
   `guildConfig.AutomatedMessagesConfig.ChannelJobs[JobType].ChannelId` — you
   only need to use `configuration.ChannelId` and an `IDiscordService` send
   method.
3. Register a cron trigger for it in
   `QuartzConfiguration.ConfigureJobs()` (`quartzServices.ScheduleJob<MyJob>(...)`).
4. Let admins configure it per-guild through the existing `job configure`
   slash command (`DiscordBot/Commands/Interactive2/Job/Configure/ConfigureJobSubCommandHandler.cs`),
   which just calls `IGroupService.SetAutomationJobChannel(jobType, user, channel, enabled)`
   → writes/updates the `ChannelJobConfiguration` entry in
   `GuildConfig.AutomatedMessagesConfig.ChannelJobs[jobType]`. No job-specific
   Discord command code is needed — the generic job-configure command handles
   any `JobType` (its `SlashCommandOptionBuilder` currently allow-lists
   `GroupUpdate`/`MonthlyTop`/`MonthlyTopGains` explicitly — a new `JobType`
   must be added to that list too, see
   `ConfigureJobSubCommandDefinition.ExtendOptionCommandBuilder`).

### `job` slash command → job wiring
`DiscordBot/Commands/Interactive2/Job/`:
- `JobRootDefinition` — root command `/job`.
- `Configure/ConfigureJobSubCommandHandler` — `/job configure <job> <channel> <enabled>`,
  requires `AuthorizationRoles.ClanAdmin`; calls
  `IGroupService.SetAutomationJobChannel(jobType, Context.User.ToGuildUserDto(), channel.ToChannelDto(), enabled)`.
  This is how `ChannelJobConfiguration` entries get created/enabled per guild —
  there is no other path to enable a `ConfigurableGuildJob` for a guild.
- `Queue/QueueSubCommandHandler` — `/job queue <job>`, requires
  `AuthorizationRoles.BotModerator`; calls `IGroupService.QueueJob(jobType)`,
  which builds a Quartz `JobBuilder`/`TriggerBuilder` on the fly and schedules
  it to fire ~5 seconds later via `ISchedulerFactory`. This bypasses the
  static cron triggers entirely — it's a manual "run it now" trigger for one
  of the three `ConfigurableGuildJob`s (`GroupUpdate`/`MonthlyTop`/`MonthlyTopGains`
  — see the `switch` in `GroupService.QueueJob`). Because `ConfigurableGuildJob`
  still iterates *every* guild and checks *that guild's own*
  `ChannelJobConfiguration`, "queue" fires the job for all guilds at once, not
  just the invoking guild.

### Config model relationships
- `GuildConfig` (`DiscordBot.Common/Models/Data/Configuration/GuildConfig.cs`,
  one LiteDB document per guild, `IGuildConfigRepository`) holds
  `AutomatedMessagesConfig AutomatedMessagesConfig { get; set; } = new();`
  among WOM group id/verification code, timezone, `CountConfig`, `CommandRoleConfig`.
- `AutomatedMessagesConfig` (`DiscordBot.Common/Models/Data/Configuration/AutomatedMessagesConfig.cs`)
  is just `Dictionary<JobType, ChannelJobConfiguration> ChannelJobs`.
- `ChannelJobConfiguration` (`.../ChannelJobConfiguration.cs`) is the per-guild,
  per-job-type record: `bool IsEnabled`, `DiscordChannelId ChannelId`,
  `DiscordGuildId GuildId`.
- `AutomatedJobState` (`.../AutomatedJobState.cs`) is a `BaseGuildModel` with a
  single `Achievement LastPrintedAchievement` field — a de-dup/watermark model
  for an achievements-announcement feature; **no job or service in this scope
  reads or writes it** (no `Achievements` job exists despite
  `JobType.Achievements = 1` being defined). Treat as scaffolding for a feature
  that was never finished.
- `JobType` enum (`DiscordBot.Common/Models/Enums/JobType.cs`): `Achievements = 1`,
  `GroupUpdate = 20`, `MonthlyTop = 40`, `MonthlyTopGains = 80` (explicit gaps,
  presumably for future bit-flag-style insertions — but it's used as a plain
  dictionary key, not a `[Flags]` enum).

## 5. Error handling & logging conventions

- **`FluentResults.Result` / `Result<T>` everywhere.** Public service and job
  methods return `Result`/`Result<T>` rather than throwing for expected
  failure paths (missing config, not-found, validation). Common patterns seen
  throughout: `Result.Ok(...)`, `Result.Fail("message")`,
  `Result.Fail(new ExceptionalError("message", exception))`,
  `.ToResult()` to downcast a `Result<T>` to `Result`, `.WithErrors(otherResult.Errors)`
  to propagate causes, and `Result.Merge(results)` in `BaseGuildJob` to combine
  many per-guild results into one.
- **Exceptions still happen** in several services (`PlayerService`,
  `GroupService`, `CountService` throw plain `Exception`/`ValidationException`
  for "no configuration found" type errors) — this is inconsistent with the
  `Result` convention above; callers/commands generally catch at the command
  handler layer. Don't assume every failure path in this codebase is
  `Result`-based — check the specific method.
- **Logging** is via `Microsoft.Extensions.Logging.ILogger` (`BaseService.Logger`,
  or `BaseJob.Logger`), backed by Serilog at the host level
  (`AddLoggingInformation` in `DiscordBot/Configuration/ConfigurationExtensions.cs`).
  Structured/templated logging is standard:
  `Logger.LogInformation("Creating new job: {@key} at {time}", jobKey, time)`,
  `Logger.LogError(exception, "... - {guid}", errorGuid)`. `BaseJob.Execute`
  wraps every job run in `Logger.BeginScope(...)` with `jobType`, `jobKey`,
  `triggerKey`, `scheduledFor`, `refire`; `BaseGuildJob` adds a nested
  per-guild `guild` scope. Errors from failed job runs are logged (not
  rethrown) inside `BaseJob.Execute`.
- Several failure-path messages include a fresh `Guid.NewGuid()` "trace id"
  that is both logged and shown to the user (e.g. `AutoUpdateGroupJob` →
  `DiscordService.SendFailedEmbed(channelId, message, errorGuid)`) so a
  support conversation can correlate a Discord-visible error with server logs.

## 6. External HTTP

- **Wise Old Man**: no direct `HttpClient` code in this scope — all access
  goes through the `WiseOldManConnector` NuGet package's typed clients
  (`IWiseOldManPlayerApi`, `IWiseOldManGroupApi`, `IWiseOldManCompetitionApi`,
  `IWiseOldManNameApi`), wrapped by `WiseOldManConnectorService`
  (`DiscordBot.Services/Services/WiseOldManConnectorService.cs`). Package-level
  logging is bridged into this app's `ILogger` via
  `WisOldManLogger : IWiseOldManLogger` (`DiscordBot.Services/Services/WisOldManLogger.cs`),
  registered `Transient`. URL-building helpers for WOM entities live in
  `DiscordBot.Services/Helpers/WiseOldManConnectorHelper.cs` (`.Url()`
  extension methods for `Group`/`Competition`/`Player`/`DeltaLeaderboard`, all
  pointed at `wiseoldman.net`) plus `.Decorate()` helpers wrapping WOM DTOs in
  `ItemDecorator<T>` (adds a display title + link).
- **OSRS Wiki**: `IOsrsWikiApi` (`DiscordBot.Services/ExternalServices/IOsrsWikiApi.cs`)
  is a `Refit` interface hitting the MediaWiki API
  (`GetPage(pageTitle)` → `/api.php?action=query&format=json&prop=revisions&titles={pageTitle}&rvprop=content&rvslots=main`,
  plus a multi-title `GetPages`). Registered in
  `ServiceConfigurationExtensions.AddExternalServices()`:
  ```csharp
  serviceCollection.AddRefitClient<IOsrsWikiApi>()
      .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://oldschool.runescape.wiki/"))
      .ConfigurePrimaryHttpMessageHandler(() => new HttpLoggingHandler());
  ```
  Response shape: `QueryResponse` → `Query` → `Pages` (dict keyed by wiki page
  id) → `Page.Revisions` → `Revision.Slots.Main.Content` (raw wikitext), all
  under `DiscordBot.Services/Models/MediaWikiApi/`. Consumed by
  `CollectionLogItemProvider` and parsed by
  `DiscordBot.Services/Parsers/MediaWikiContentToItemsParser.cs` (regex
  `{{plink|item|...}}` → item name list, with a `txt=` override).
- **`HttpLoggingHandler`** (`DiscordBot.Services/HttpClients/HttpJoggingHandler.cs`,
  filename typo — class is `HttpLoggingHandler`, note the file itself is named
  `HttpJoggingHandler.cs`): a `DelegatingHandler` that dumps method/URL/headers/
  body (truncated to 255 chars, only for text-like content types) and timing
  for both request and response to `System.Diagnostics.Debug.WriteLine` — **not**
  wired to `ILogger`/Serilog, so it's only visible when debugging with a
  debugger attached, not in production logs.

## 7. Gotchas for an AI editing this code

- **`IJobService` has no implementation.** Don't call it expecting it to work;
  either implement it or use `IRepositoryStrategy`/`IGuildConfigRepository`
  directly as the existing jobs do.
- **`ConfigurableGuildJob` silently no-ops per guild** if that guild has no
  `ChannelJobConfiguration` entry for the `JobType` — there's no log line for
  "skipped, not configured", so a job that appears to "do nothing" for a given
  guild is very likely just unconfigured there, not broken. Only
  `AutoUpdateGroupJob` additionally checks `IsEnabled`; if you add a new
  `ConfigurableGuildJob` remember to add the `IsEnabled` check yourself in
  `DoWorkForGuildWithContext` — the base class does **not** do it for you.
- **In-memory Quartz store** (`UseInMemoryStore()`): dynamically scheduled jobs
  (`GroupService.QueueJob`, `AutomatedDropperService.ScheduleJob`) do not
  survive a process restart. Only the four jobs wired in
  `QuartzConfiguration.ConfigureJobs()` come back automatically on boot.
- **`HandleRunescapeDropJob.GetGuildIdsForEndpoint` is hardcoded** to a single
  guild id (`403539795944538122`) rather than looking up which guilds the
  endpoint's user actually belongs to — this looks like unfinished
  multi-tenant support, not a deliberate single-guild design; be careful
  assuming this generalizes.
- **Drop filtering (`HandleRunescapeDropJob.FilterData`) is commented out** —
  every non-pet drop is currently sent unfiltered to every configured channel;
  the rich `DropperConfiguration` allow/deny-list model exists in data but has
  no effect at runtime.
- **`AutomatedJobState`/`JobType.Achievements`** exist as data model/enum but
  have no reader/writer or job anywhere in scope — do not build on the
  assumption an achievements job runs.
- **Two different "run this job now" paths** exist for the three
  `ConfigurableGuildJob`s: the static cron triggers in `QuartzConfiguration`,
  and `GroupService.QueueJob`/the `/job queue` command, which schedules a new
  ad-hoc one-shot `JobKey` (`Guid.NewGuid()`, no group) that still iterates
  *all* guilds — there is no per-guild "run just for my guild" trigger.
- **Error-handling style is inconsistent**: some services throw
  (`PlayerService`, `GroupService`, `CountService`'s private
  `GetGroupConfigWithValidCountConfig`) while others strictly return
  `Result`/`Result<T>` (`GraveyardService`, `ClanFundsService`,
  `ConfirmationService`, `AuthorizationService`). Check the specific method
  before assuming a try/catch vs. `Result.IsFailed` check is the right way to
  handle its failure.
- **`HttpLoggingHandler` only writes to `Debug.WriteLine`**, not `ILogger` —
  don't rely on it for production diagnostics of wiki API calls.
- **`IDiscordService` is `Transient`, `DiscordSocketClient` is `Singleton`** —
  creating a new `DiscordService` instance is cheap and always talks to the
  same live gateway cache; there's no per-request Discord connection.
- **Guild/channel/user lookups depend on the socket cache being populated**
  (`AlwaysDownloadUsers = true` is set in `AddDiscordClient`, but
  `GetGuilds()` still explicitly fails if `ConnectionState != Connected` —
  code calling into `IDiscordService` early in startup, before the gateway
  handshake completes, will get `Result.Fail`, not stale/empty data).
- **Tests coverage is narrow** (`tests/DiscordBot.ServicesTests`): only
  `AutomatedDropperService` (debounce/reschedule behavior with mocked
  `IScheduler`/`IRepositoryStrategy`), `CollectionLogItemProvider` (caching +
  reset, using a fixtured wiki JSON response), `MediaWikiContentToItemsParser`
  (regex edge cases: apostrophes, dashes, numbers, braces, `txt=` override),
  `MediaWikiJsonResponseToQueryResponse` deserialization, `DateTimeHelper`
  (timezone offset conversion, several skipped/known-incorrect DST cases),
  `OsrsWikiApi` (live integration test hitting the real wiki), a `Humanizer`
  date-parsing helper test, and `IdentityTests` (LiteDB + strongly-typed id
  round-tripping, unrelated to services). **No tests exist for**
  `CountService`, `GroupService`, `PlayerService`, `GraveyardService`,
  `ClanFundsService`, `ConfirmationService`, `AuthorizationService`, or any of
  the Quartz job classes (`AutoUpdateGroupJob`, `TopLeaderBoardJob`,
  `MonthlyTopDeltasJob`, `HandleRunescapeDropJob`) — changes there have no
  regression safety net.
