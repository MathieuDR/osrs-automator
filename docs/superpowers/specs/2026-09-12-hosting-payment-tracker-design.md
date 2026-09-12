# Hosting payment tracker ("public shaming")

Date: 2026-09-12 (rev 3, after owner feedback) · Status: **implemented on feature/hosting-tracker-and-litedb-lifecycle** · Scope: new `/hosting` command with guild autocomplete, one common-DB collection, one Quartz job, a footer hook in the reply pipeline, configurable shaming messages, and a "degraded mode" that randomly fails commands once a server is very overdue.

Background: `docs/codebase/02-command-system.md` (commands, authorization, autocomplete, reply builders), `03-data-layer.md` (common DB, repositories), `04-services-and-jobs.md` (Quartz, `IDiscordService`).

## 1. What the owner asked for

- From the owner's "manager" server (guild `403539795944538122`), record **per clan server** when that server paid for hosting, and see which servers have paid and which have not.
- Remind the owner **once a month before** the due date and **once on the due date**. No repeated nagging of the owner afterwards.
- Every command reply in a clan server carries a small footer with that server's payment status, worded to **publicly shame** the clan when it is late.
- A way to enable/disable it.
- After ~90 days overdue, "break" the bot a little: roughly 1 in 5 commands answers with a funny failure and asks the user to retry. Random per command (20 % chance), not a counter.
- Several texts per shaming tier, picked at random, for variety.

## 2. Assumptions (owner: correct any of these)

1. **Payment state is per guild**, stored centrally in the **common** LiteDB file so the owner can list every server from one place. The footer shown in guild X reflects guild X's record only.
2. Only `Bot:TeamConfiguration:OwnerId` can use `/hosting`, and only inside `Bot:TeamConfiguration:GuildId` (must be `403539795944538122` in production settings; the checked-in default is `0`).
3. Default term is **12 months**, overridable per payment.
4. Footer on **command replies only**, not on scheduled job posts, and **only while the guild is overdue** (`DaysOverdue >= 0`). Paid-up guilds, guilds in their last month, and commands used in DMs get no footer. A guild with no payment recorded at all is treated as silent by default (open question 3).
5. Owner reminders go to a channel in the owner guild with an @mention (default: the channel where `/hosting paid` was last run). No DMs.
6. Shaming texts are **configuration**, not code, so wording can be tuned without a deploy (§3.6).

## 3. Components

### 3.1 Data: `GuildHostingState` (common DB, one record per guild)

`DiscordBot.Common/Models/Data/Hosting/GuildHostingState.cs`

```csharp
public record GuildHostingState : BaseRecord {
    public DiscordGuildId GuildId { get; init; }
    public bool FooterEnabled { get; init; } = true;
    public List<HostingPayment> Payments { get; init; } = new();   // newest last
    public DateTime? UpcomingReminderSentForDueOn { get; init; }    // which due date the "30 days left" reminder covered
    public DateTime? DueReminderSentForDueOn { get; init; }         // which due date the "due today" reminder covered
}

public record HostingPayment {
    public DateTime PaidOn { get; init; }      // date only, UTC midnight
    public int TermMonths { get; init; }       // default 12
    public string? Note { get; init; }         // e.g. "paid by Bob, 20 EUR"
    public DiscordUserId RecordedBy { get; init; }
}
```

Plus one bot-wide record for owner settings, `HostingSettings : BaseRecord { DiscordChannelId? ReminderChannelId; }` (single record, same collection family).

Derived, never stored: `DueOn = LastPayment.PaidOn.AddMonths(TermMonths)`, `DaysOverdue = (today - DueOn).Days` (negative = days left). Storing the "sent for due date" instead of a boolean means a new payment (new `DueOn`) automatically re-arms both reminders.

Repositories, following `ApplicationCommandInfoRepository` (recipe in `03-data-layer.md` §7):

- `IGuildHostingStateRepository : IRecordRepository<GuildHostingState>` with `Result<GuildHostingState?> GetByGuildId(DiscordGuildId)`; collection `"guildHosting"`; index on `GuildId` (`EnsureIndex` in the constructor, unique).
- `IHostingSettingsRepository : ISingleRecordRepository<HostingSettings>`; collection `"hostingSettings"`.
- Two factories with `RequiresGuildId => false` using the common database; register each in `DiscordBot.Data/Configuration/ConfigurationExtensions.cs` **both** in the `AddTransient` list and the `RepositoryStrategy` array (missing the array entry fails at runtime with "not registered").
- `DiscordGuildId`/`DiscordUserId`/`DiscordChannelId` are structs with mappers registered in `LiteDbManager.AddMappers`; `DiscordChannelId?` (nullable struct) needs a round-trip test, fall back to `long?` if LiteDB does not map it.

If the LiteDB lifecycle spec lands first, repositories are `IDisposable`; write every use as `using var repo = …` from the start.

### 3.2 Service: `IHostingService` (`DiscordBot.Services`, singleton)

```csharp
public interface IHostingService {
    HostingStatus GetStatus(DiscordGuildId guildId);                       // cached; never throws
    Result<IReadOnlyList<HostingStatus>> GetOverview(IEnumerable<Guild> guilds); // one row per guild the bot is in
    Result RecordPayment(DiscordGuildId guildId, DateOnly paidOn, int termMonths, string? note, DiscordUserId by);
    Result SetFooterEnabled(DiscordGuildId guildId, bool enabled);
    Result SetReminderChannel(DiscordChannelId channel);
    Result<DiscordChannelId?> GetReminderChannel();
    Result MarkUpcomingReminderSent(DiscordGuildId guildId, DateOnly dueOn);
    Result MarkDueReminderSent(DiscordGuildId guildId, DateOnly dueOn);
}

public record HostingStatus(DiscordGuildId GuildId, string GuildName, bool FooterEnabled,
                            HostingPayment? LastPayment, DateOnly? DueOn, int? DaysOverdue) {
    public string? FooterText { get; init; }   // null unless overdue, enabled, and in a guild
}
```

- **Singleton** (other services are transient) because it caches all `GuildHostingState` records in a `ConcurrentDictionary<DiscordGuildId, GuildHostingState>` loaded on first use and updated on every write. Reads happen on every command reply; writes only come through this service; the bot is one process, so this is safe. Day counts are recomputed per call so the footer rolls over at midnight.
- Takes a `TimeProvider` (or `Func<DateTime>`) for "today" so tests can pin dates.
- Formats `FooterText` from `HostingMessagesConfiguration` (§3.6) so the `status` command and the footer always agree.
- `GetStatus` catches and logs repository exceptions and returns a status with `FooterText = null`; a broken common DB must not break every command.
- Registered in `ServiceConfigurationExtensions.AddDiscordBotServices`.

### 3.3 Command: `/hosting` (Interactive2, owner only, owner guild only)

Folder `DiscordBot/Commands/Interactive2/Hosting/`, shapes as in `Ping/`, `Job/Configure/` and the autocomplete pair in `Graveyard/ShameLocationAutoCompleteHandler.cs`.

| subcommand | options | behaviour |
|---|---|---|
| `paid` | `server` (string, required, **autocomplete**), `date` (string, optional `yyyy-mm-dd`, default today), `months` (integer, optional, default 12, min 1 max 60), `note` (string, optional) | appends a payment for that guild; if no reminder channel is set yet, sets it to the current channel; replies ephemeral with the new due date |
| `status` | `server` (string, optional, autocomplete) | without `server`: overview table of **every guild the bot is in** (name, last paid, due, days left/overdue, footer on/off), sorted most-overdue first, paginated if > 20 rows. With `server`: that guild's details and payment history |
| `footer` | `server` (autocomplete, required), `enabled` (boolean, required) | toggles that guild's footer |
| `remind` | `channel` (channel, required) | sets the owner reminder channel |

- `HostingRootCommandDefinition : RootCommandDefinitionBase`, `Name => "hosting"`, `builder.WithDMPermission(false)`.
- **Guild autocomplete**: Discord has no "guild" option type, so `server` is a string option with `.WithAutocomplete(true)`. `HostingGuildAutoCompleteHandler : AutoCompleteHandlerBase<…>` reads `DiscordSocketClient.Guilds`, filters by `Context.CurrentOptionAsString` (case-insensitive contains on the name), and responds with up to 25 choices where the **label is the guild name and the value is the guild id as a string**. Handlers parse the value with `ulong.TryParse`; if it fails (user typed free text), fall back to a name match, else `Result.Fail("Unknown server")`.
- Every request: `MinimumAuthorizationRole => AuthorizationRoles.BotOwner`. `CommandAuthorizationService` grants that only to `OwnerId` (the `roleRequired <= BotModerator` branch). No new auth code.
- Every handler's `DoWork` first checks `Context.Guild?.Id == BotTeamConfiguration.GuildId`, else `Result.Fail("Only available in the bot owner's server")`.
- Date parsing with `DateOnly.TryParseExact("yyyy-MM-dd")`; failures return `Result.Fail` and the dispatcher renders the ephemeral red embed.
- Overview table rendering: reuse `InteractionPaginatorReplyBuilder` with a code-block table (see `02-command-system.md` §6 / `04` leaderboard formatting).

**Registering in Discord (one-time):** deploy, run `/commands` in the owner guild, pick `hosting`, choose the owner guild only. Restarts re-sync afterwards.

### 3.4 Footer hook

All Interactive2 replies, paginator pages, and the dispatcher's failure embed build through `BaseInteractiveContext<T>` (`DiscordBot/Models/Contexts/BaseInteractiveContext.cs`). In its constructor, if `InGuild`, resolve `IHostingService` from the `IServiceProvider` it already receives and store `FooterText = GetStatus(Guild.Id).FooterText` once per interaction (`null` in DMs). Then:

1. `CreateEmbedBuilder(title, content)`: pass `FooterText` as the existing `appendToFooter` argument of `WithMessageAuthorFooter` (`EmbedBuilderHelper.cs`); change the separator from `", "` to `" · "`.
2. `CreatePageBuilder(...)`: `.WithFooter(FooterText)` when non-null (Fergun appends page numbers; total stays far under the 2048-char limit).
3. `RespondAsync`/`FollowupAsync` overrides: when there are no embeds and `FooterText` is non-null, append `"\n-# " + FooterText` to the text (Discord subtext markdown). Covers text-only replies like `ping2 normal`.

`AutocompleteCommandContext` never builds embeds; the constructor lookup is a dictionary read, so no measurable cost there.

Not covered: the ~13 sites that build `new EmbedBuilder()` directly (leaderboard converters, `DiscordService`). Optional follow-up.

### 3.5 Owner reminders: `HostingReminderJob`

`DiscordBot.Services/Jobs/HostingReminderJob : BaseJob`, scheduled in `ServiceConfigurationExtensions.ConfigureJobs` daily at 09:00 `Europe/Berlin`, `WithMisfireHandlingInstructionFireAndProceed`, identity `"hosting-reminder"` / group `"hosting"`.

For each guild the bot is in (`IDiscordService.GetGuilds()`) with at least one payment:

| condition | action |
|---|---|
| reminder channel not set | log once at Warning, skip everything |
| `DaysOverdue == -30` **or** (`-30 < DaysOverdue < 0` and `UpcomingReminderSentForDueOn != DueOn`) | send "due in N days" reminder, `MarkUpcomingReminderSent` |
| `DaysOverdue >= 0` and `DueReminderSentForDueOn != DueOn` | send "due today / overdue" reminder, `MarkDueReminderSent` |
| anything else | nothing |

That is exactly two messages per due date, and none after the due-date one, until a new payment sets a new `DueOn`. The "or" clause covers a missed 09:00 run (bot down that day). Multiple guilds due the same day are combined into one embed with one field per guild, one @mention.

Message: content `<@OwnerId>` plus an embed titled `Hosting payments`, one field per guild: `{name}: due {date} ({N days left | N days overdue})`, last note. Add to `IDiscordService`:

```csharp
Task<Result> SendMentionEmbed(DiscordChannelId channelId, DiscordUserId mention, EmbedBuilder embed);
```

implemented next to the private `SendEmbed` with `SendMessageAsync($"<@{mention.UlongValue}>", embed: embed.Build(), allowedMentions: AllowedMentions.All)`.

`BotTeamConfiguration` lives in the `DiscordBot` project, which `DiscordBot.Services` does not reference (verified in the csproj files). Move it to `DiscordBot.Common/Configuration/` next to `MessageConfiguration`; fix the two `using`s in `CommandAuthorizationService` and `InteractiveCommandHandlerService`.

### 3.6 Shaming texts: `Bot:Messages:Hosting` configuration

Extend `MessageConfiguration` (`DiscordBot.Common/Configuration/MessageConfiguration.cs`, already bound from `Bot:Messages` and registered as a singleton) with:

```csharp
public HostingMessages Hosting { get; set; } = new();

public class HostingMessages {
    public List<HostingTier> Overdue { get; set; }   // pick the tier with the largest MinDays <= DaysOverdue, then a random text from it
    public List<string>? NeverPaid { get; set; }     // null or empty = stay silent for guilds without a record
    public DegradedMode Degraded { get; set; } = new();
}
public class HostingTier { public int MinDays { get; set; } public List<string> Texts { get; set; } = new(); }
public class DegradedMode {
    public bool Enabled { get; set; } = true;
    public int MinDaysOverdue { get; set; } = 90;
    public double FailureChance { get; set; } = 0.2;   // 0..1, evaluated independently per command
    public List<string> Texts { get; set; } = new();
}
```

Placeholders: `{server}`, `{days}` (absolute value), `{date}` (due date, `d MMM yyyy`), `{paidDate}`. Suggested defaults for `appsettings.default.json` (owner will want to edit these):

| tier | example texts (each tier is a list; one is picked at random per reply) |
|---|---|
| `Overdue[0]` (0 d) | `💸 {server} has not paid its hosting bill. Due {date}. The bot runs on goodwill now.` · `🧾 Hosting invoice for {server}: unpaid since {date}.` · `💸 Day {days} of {server} not paying for hosting. Just saying.` |
| `Overdue[1]` (7 d) | `😬 {days} days without paying for hosting. Every command you run costs someone else money.` · `📉 {server} is {days} days late on hosting. The bot noticed. So did everyone else.` · `🪙 {days} days overdue. A few coins from each of you would fix this.` |
| `Overdue[2]` (30 d) | `🚨 {days} DAYS OVERDUE. This bot is being kept alive out of pity. Pay the man.` · `⏳ {days} days. At this point the bot is a charity case.` · `🚨 Month-plus overdue. Leadership, this footer is for you.` |
| `Overdue[3]` (90 d) | `☠️ {days} days. {server} is officially freeloading. Somebody screenshot this for the leaders.` · `🪦 {days} days overdue. The bot is running on fumes and spite.` · `☠️ Quarter of a year unpaid. Commands may start "failing". Coincidence.` |
| `NeverPaid` | *(empty list by default; example if wanted:* `🤔 {server} has never paid for hosting. Awkward.`*)* |

The footer is **only rendered while `DaysOverdue >= 0`**; there is deliberately no "paid" or "due soon" footer, so a clan that pays on time never sees the bot mention money. The `status` command still shows the full picture to the owner. A guild with no record shows `NeverPaid` only if that text is configured non-empty (default: silent).

### 3.7 Degraded mode: random "funny" failures when very overdue

Once a guild is `Degraded.MinDaysOverdue` (default 90) or more days overdue, each slash command in that guild has a `Degraded.FailureChance` (default 0.2) probability of not running at all and instead getting a short, funny failure that tells the user to retry. Independent random draw per command (`Random.Shared.NextDouble() < FailureChance`), no counters, no state.

**Hook point:** `InteractiveCommandHandlerService.OnInteraction` (`DiscordBot/Services/InteractiveCommandHandlerService.cs`), right after the context is built and before `_commandInstigator.ExecuteCommandAsync`. Both command systems pass through there, so legacy commands are covered too. Add to `IHostingService`:

```csharp
bool ShouldDegrade(DiscordGuildId guildId);   // true => skip the command and reply with a degraded message
string GetDegradedMessage(DiscordGuildId guildId);
```

`ShouldDegrade` returns `false` when: degraded mode is disabled, the guild is not overdue enough, `FooterEnabled` is `false` for that guild (the per-guild toggle is the kill switch for all shaming), the interaction is in the owner guild, the user is `OwnerId`, or the command is `hosting`. It also only applies to `SocketSlashCommand` interactions: autocomplete requests and button clicks (confirmation flows, paginators) are never degraded, so a failed command never leaves a half-finished flow behind.

**Reply:** `ctx.CreateEmbedBuilder().WithFailure(message)` sent with `RespondAsync(ephemeral: false)`, so the whole channel sees it and the overdue footer is appended automatically by §3.4. Log at Information with guild id and command name so the owner can see it happening.

**Rule for every degraded text (owner decision):** it must state plainly that the command was withheld because hosting is unpaid, and must never imply a technical fault. Users should know nothing is broken.

Default `Degraded.Texts` (placeholders as in §3.6):

- `💸 Not running that one: {server} has not paid for hosting in {days} days. Nothing is broken. Retry, or pay.`
- `🛑 Command withheld: this server's hosting bill is {days} days overdue. Retry, or nudge whoever holds the clan coffers.`
- `💤 The bot works when the hosting gets paid. {days} days overdue. Retry in a moment.`
- `🧾 Unpaid hosting ({days} days) means about 1 in 5 commands takes a nap. This was one of them. Retry.`
- `🎲 Rolled the unpaid-hosting dice and lost. Not a bug, just {days} days of unpaid hosting. Retry.`

Effort: about 40 lines (service method, one `if` in `OnInteraction`, config class, tests). The random draw is injected as `Func<double>` (default `Random.Shared.NextDouble`) so tests can force 0.0 and 1.0.

## 4. Data flow

```
/hosting paid server:<id> ─► PaidHandler ─► IHostingService.RecordPayment ─► common DB (guildHosting) + cache
any command in guild X ─► OnInteraction: IHostingService.ShouldDegrade(X)? ─► yes (20 %, ≥90 d): funny failure + footer, stop
                        └► no ─► BaseInteractiveContext ctor ─► IHostingService.GetStatus(X).FooterText
        ├─ CreateEmbedBuilder → "Requested by Bob · 💸 Clan X has not paid …"
        ├─ CreatePageBuilder  → paginator footer
        └─ RespondAsync(text) → "\n-# 💸 Clan X has not paid …"
09:00 daily ─► HostingReminderJob ─► for each guild: due in 30d? due today? ─► SendMentionEmbed(owner channel) ─► Mark…Sent
```

## 5. Error handling

- Service methods return `FluentResults.Result`; handlers propagate; the dispatcher renders failures as ephemeral embeds.
- `GetStatus` never throws (logs + null footer).
- Reminder send failure: log, return `Result.Fail`, do **not** mark as sent, so tomorrow's run retries.

## 6. Tests (`tests/DiscordBot.ServicesTests`)

- Footer text selection: not overdue → null; each overdue tier boundary; never paid with and without configured text; disabled → null; placeholder substitution; all with a fixed "today".
- `AddMonths` edge cases (31 Jan + 1 month, 29 Feb).
- Reminder decision table (§3.5) as a `[Theory]` over (`DaysOverdue`, `UpcomingReminderSentForDueOn`, `DueReminderSentForDueOn`) → expected sends; plus "new payment re-arms reminders".
- Repository round trip through the `LiteDbManager` test harness (`IdentityTests.cs` pattern) including the unique `GuildId` index and `DiscordChannelId?`.
- Autocomplete value parsing (`ulong` id, name fallback, unknown).
- Degraded mode: forced draw 0.0 always degrades and 1.0 never does once ≥ `MinDaysOverdue`; never degrades below the threshold, in the owner guild, for the owner, for `hosting`, or when the guild footer is disabled; random text selection covers every configured text.
- Tier text selection picks only from the matching tier and, with a seeded random, cycles through all texts.
- `IDiscordService` mocked with NSubstitute in job tests.

## 7. Implementation order

1. Models, repositories, factories, registration, round-trip test.
2. `MessageConfiguration.Hosting` + defaults in `appsettings.default.json`.
3. `IHostingService` + formatting/decision tests.
4. Footer hook in `BaseInteractiveContext` and the degraded-mode check in `OnInteraction`; verify in a dev guild with `ping2 normal` and an embed command, with a payment backdated 4 months.
5. `/hosting` command (four subcommands + autocomplete); register via `/commands` in the owner guild.
6. Move `BotTeamConfiguration` to Common; `SendMentionEmbed`; `HostingReminderJob` + schedule; test by recording a payment dated 11 months ago.
7. Update `docs/codebase/02`, `03`, `04`.

Config: the owner confirmed production `appsettings` sets `Bot:TeamConfiguration` (`GuildId`, `OwnerId`) and `LiteDbOptions:FileSuffix = "prod"`, and does **not** override `Bot:Messages`. `Program.BuildConfig` layers `appsettings.json` then `appsettings.{env}.json`, so the `Hosting` defaults go into the base `appsettings.json` and the production file only needs an override when the owner wants different wording.

## 8. Open questions for the owner

1. Overdue tiers and wording in §3.6: keep, soften, or sharpen? They are config, so this is not blocking.
2. Should the bot also post one public message **in the clan server** on the due date (e.g. in that guild's configured job channel), or is the footer enough? Default here: footer only.
3. Guilds with no payment recorded: stay silent (default) or show a `NeverPaid` footer? Set the text in config to opt in.
4. 12-month default term: fine?
5. Degraded-mode failures are public (not ephemeral) so the channel sees them. Keep, or make them ephemeral?
6. Degraded mode defaults: 90 days and 20 %. Both are config.

## 9. Implementation notes / deviations

Where the built code differs from the design above (all committed on
`feature/hosting-tracker-and-litedb-lifecycle`; see `docs/codebase/02-command-system.md`,
`03-data-layer.md`, `04-services-and-jobs.md` for the fuller write-up):

- **`SendMentionEmbed` takes plain data, not an `EmbedBuilder`.** §3.5 sketched
  `Task<Result> SendMentionEmbed(DiscordChannelId channelId, DiscordUserId mention, EmbedBuilder embed)`.
  The shipped signature is
  `Task<Result> SendMentionEmbed(DiscordChannelId channelId, DiscordUserId mention, string title, EmbedFieldDto[] fields, bool isAlert)`
  — `HostingReminderJob` lives in `DiscordBot.Services`, which has no reference to `Discord.Net`/
  `EmbedBuilder` (verified in the csproj files, same constraint §3.5 already called out for
  `BotTeamConfiguration`), so the embed is built entirely inside `DiscordService`. `isAlert` picks red vs.
  orange (red when any guild in the batch is overdue, i.e. `ReminderKind.Due`).
- **`HostingStatus` has no `GuildName`.** §3.2 sketched
  `record HostingStatus(DiscordGuildId GuildId, string GuildName, bool FooterEnabled, HostingPayment? LastPayment, DateOnly? DueOn, int? DaysOverdue)`.
  The shipped shape (`DiscordBot.Services/Models/HostingStatus.cs`) is
  `record HostingStatus(DiscordGuildId GuildId, bool FooterEnabled, HostingPayment? LastPayment, DateOnly? DueOn, int? DaysOverdue, string? FooterText)`
  — no `GuildName` field; `IHostingService.GetStatus`/`GetOverview` instead take an optional `guildName`
  *parameter* (used only for `{server}` substitution in `FooterText`/degraded messages, falling back to
  `guildId.ToString()`), and `FooterText` is a constructor parameter rather than a computed `init` property.
- **`HostingSettings.ReminderChannelId` is `long?`, not `DiscordChannelId?`.** §3.1 asked for
  `DiscordChannelId?` with a fallback to `long?` "if LiteDB does not map it" (nullable-struct round-trip
  needed a test). The shipped model stores `long?` directly and converts to `DiscordChannelId` only at the
  `HostingService` boundary (`GetReminderChannel`/`SetReminderChannel`) — the nullable-struct BSON mapper
  path was not attempted.
- **Date storage rule**: every hosting date (`HostingPayment.PaidOn`, `UpcomingReminderSentForDueOn`,
  `DueReminderSentForDueOn`) is stored as UTC midnight (`HostingDates.ToStorage(DateOnly)`:
  `DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)`) and read back as a
  `DateOnly` via `HostingDates.ToDateOnly(DateTime)` (`stored.ToUniversalTime()` then
  `DateOnly.FromDateTime`) — needed because LiteDB is configured with `UtcDate=false`, so a round-tripped
  `DateTime` comes back `Local`-kind for the same instant; comparing raw `DateTime`s (rather than the
  `DateOnly` this helper produces) would be timezone-fragile. `HostingReminderJob.Decide` and
  `HostingService` both go through `HostingDates`, never comparing raw `DateTime`s directly.
- **`IClock` instead of `TimeProvider`.** §3.2 suggested `TimeProvider` (or `Func<DateTime>`) for "today".
  The target framework is net7.0, which predates `TimeProvider` (introduced in .NET 8), so a minimal
  `IClock { DateTime UtcNow { get; } }`/`SystemClock` seam was rolled instead
  (`DiscordBot.Services/Helpers/IClock.cs`), registered `AddSingleton<IClock, SystemClock>()`.
- **`internal DoWorkForTests()` seam on `HostingReminderJob`.** `BaseJob.DoWork` is `protected` (invoked by
  Quartz via `Execute(IJobExecutionContext)`); rather than construct a fake `IJobExecutionContext` in
  tests, the job exposes `internal Task<Result> DoWorkForTests() => DoWork()`, reachable from
  `DiscordBot.ServicesTests` via `InternalsVisibleTo`.
- **Reminder decision table condition is phrased slightly differently than §3.5's table**, though
  behaviourally equivalent for the "misfired 09:00 run" case: the shipped `Decide` uses
  `daysOverdue is >= -30 and < 0` (a closed range) for the "upcoming" condition rather than
  `DaysOverdue == -30 or (-30 < DaysOverdue < 0 and ...)`; both admit every day from 30-days-out through
  the day before due, gated by "not already sent for this `DueOn`" either way.
