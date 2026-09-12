# Hosting Payment Tracker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Owner records per-server hosting payments from their manager server; overdue servers get an escalating shaming footer on command replies and, after 90 days, random "funny failure" degraded mode; the owner gets exactly two reminders per due date.

**Architecture:** One common-DB collection (`guildHosting`, one record per guild) plus a single settings record; a singleton `HostingService` in `DiscordBot.Services` caches state and owns all decisions (footer text, tiers, degrade, reminders) so they are unit-testable; the `DiscordBot` project adds thin hooks (context footer, dispatcher degrade check), the `/hosting` slash command with guild autocomplete, and a daily Quartz reminder job.

**Tech Stack:** .NET 7 (no `TimeProvider`; use the `IClock` defined in Task 3), Discord.Net 3.10, MediatR command pipeline (`Interactive2`), LiteDB via the lease API from the LiteDB lifecycle plan (repositories are `IDisposable`; always `using var`), Quartz, xUnit + FluentAssertions + NSubstitute.

**Spec:** `docs/superpowers/specs/2026-09-12-hosting-payment-tracker-design.md`. Read it fully, plus `docs/codebase/02-command-system.md` (§2, §5, §7 recipe), `03-data-layer.md` (§7 recipe), `04-services-and-jobs.md` (§2 `IDiscordService`, §4 jobs).

## Global Constraints

- Owner guild and owner user come from `Bot:TeamConfiguration` (`GuildId`, `OwnerId`); never hard-code `403539795944538122`.
- Footer only when `DaysOverdue >= 0` and the guild's `FooterEnabled` is true and the interaction is in a guild. Never in DMs, never for autocomplete.
- Degraded mode only for `SocketSlashCommand` interactions, never in the owner guild, never for the owner user, never for the `hosting` command, never when the guild's footer is disabled.
- Reminders: exactly one "30 days left" and one "due" message per due date.
- Every degraded-mode text must state plainly that the command was withheld because hosting is unpaid and must never imply a technical fault (owner decision; the defaults are in spec §3.7).
- All shaming/degraded texts have code defaults and are overridable from `Bot:Messages:Hosting`; a missing config section must not break startup.
- Every repository use is `using var repo = …` (repositories are `IDisposable`).
- Build: `dotnet build OrsrDiscordAutomator.sln`; test: `dotnet test tests/DiscordBot.ServicesTests/DiscordBot.ServicesTests.csproj`. Do not increase the 5 pre-existing failures.
- Commit per task on `feature/hosting-tracker-and-litedb-lifecycle`; messages end with `Co-Authored-By: Claude Fable 5.1 <noreply@anthropic.com>`.
- TDD per task: test first, see it fail, implement, pass, commit. Follow existing test conventions in `tests/DiscordBot.ServicesTests`.

---

### Task 1: Move `BotTeamConfiguration` to `DiscordBot.Common`

**Files:**
- Move: `DiscordBot/Configuration/BotTeamConfiguration.cs` → `DiscordBot.Common/Configuration/BotTeamConfiguration.cs`, namespace `DiscordBot.Common.Configuration`. Content unchanged (`DiscordGuildId GuildId`, `DiscordUserId OwnerId`).
- Modify: `DiscordBot/Configuration/BotConfiguration.cs` (property type reference), `DiscordBot/Services/CommandAuthorizationService.cs`, `DiscordBot/Services/InteractiveCommandHandlerService.cs`, `DiscordBot/Configuration/ConfigurationExtensions.cs` (`using DiscordBot.Common.Configuration;`). `grep -rn BotTeamConfiguration --include=*.cs` for the full list, including `DiscordBot.Dashboard`.

- [ ] Build the solution, run tests (no change expected), commit `refactor: move BotTeamConfiguration to DiscordBot.Common`.

### Task 2: Models, repositories, factories, registration

**Files:**
- Create: `DiscordBot.Common/Models/Data/Hosting/GuildHostingState.cs`, `HostingPayment.cs`, `HostingSettings.cs`
- Create: `DiscordBot.Data/Interfaces/IGuildHostingStateRepository.cs`, `IHostingSettingsRepository.cs`
- Create: `DiscordBot.Data/Repository/GuildHostingStateRepository.cs`, `HostingSettingsRepository.cs`
- Create: `DiscordBot.Data/Factories/GuildHostingStateRepositoryFactory.cs`, `HostingSettingsRepositoryFactory.cs`
- Modify: `DiscordBot.Data/Configuration/ConfigurationExtensions.cs` (add both factories to the `AddTransient` list **and** the `RepositoryStrategy` array)
- Create: `tests/DiscordBot.ServicesTests/Data/HostingRepositoryTests.cs`

**Produces:**

```csharp
// DiscordBot.Common.Models.Data.Hosting
public record GuildHostingState : BaseRecord {
    public DiscordGuildId GuildId { get; init; }
    public bool FooterEnabled { get; init; } = true;
    public List<HostingPayment> Payments { get; init; } = new();
    public DateTime? UpcomingReminderSentForDueOn { get; init; }
    public DateTime? DueReminderSentForDueOn { get; init; }
}
public record HostingPayment {
    public DateTime PaidOn { get; init; }
    public int TermMonths { get; init; } = 12;
    public string? Note { get; init; }
    public DiscordUserId RecordedBy { get; init; }
}
public record HostingSettings : BaseRecord {
    public long? ReminderChannelId { get; init; }   // stored as long? to avoid nullable-struct mapper questions; convert at the service boundary
}

// DiscordBot.Data.Interfaces
public interface IGuildHostingStateRepository : IRecordRepository<GuildHostingState> {
    Result<GuildHostingState?> GetByGuildId(DiscordGuildId guildId);
}
public interface IHostingSettingsRepository : ISingleRecordRepository<HostingSettings> { }
```

Repositories: `GuildHostingStateRepository : BaseRecordLiteDbRepository<GuildHostingState>` with `CollectionName => "guildHosting"`, constructor calls `GetCollection().EnsureIndex(x => x.GuildId, unique: true)`, `GetByGuildId` = `GetCollection().Query().Where(x => x.GuildId == guildId).FirstOrDefault()`. `HostingSettingsRepository : BaseSingleRecordLiteDbRepository<HostingSettings>` with `CollectionName => "hostingSettings"`. Factories copy `CommandInfoRepositoryFactory` (`RequiresGuildId => false`, `Create()` uses `LiteDbManager.LeaseCommon()`).

- [ ] **Tests** (fixture like `IdentityTests`, build the factory directly; `InternalsVisibleTo` exists from the LiteDB plan):
  - `GuildHostingState_RoundTrips`: insert a state with two payments (one with a note), both reminder dates set, `FooterEnabled=false`; dispose repo; new repo `GetByGuildId` → `BeEquivalentTo` excluding `Id`/`CreatedOn`.
  - `GetByGuildId_UnknownGuild_ReturnsNull`.
  - `GuildId_IsUnique`: inserting a second state for the same guild throws `LiteException`.
  - `HostingSettings_RoundTrips` with `ReminderChannelId = 123` and with `null`.
- [ ] Implement, build, run tests, commit `feat(hosting): models and repositories`.

### Task 3: `HostingMessages` config, `IClock`, and `HostingService`

**Files:**
- Modify: `DiscordBot.Common/Configuration/MessageConfiguration.cs` (add `public HostingMessages Hosting { get; set; } = new();`)
- Create: `DiscordBot.Common/Configuration/HostingMessages.cs`
- Create: `DiscordBot.Services/Helpers/IClock.cs` (`public interface IClock { DateTime UtcNow { get; } }`, `public sealed class SystemClock : IClock { public DateTime UtcNow => DateTime.UtcNow; }`)
- Create: `DiscordBot.Services/Interfaces/IHostingService.cs`, `DiscordBot.Services/Models/HostingStatus.cs`, `DiscordBot.Services/Services/HostingService.cs`
- Modify: `DiscordBot.Services/Configuration/ServiceConfigurationExtensions.cs` `AddServices`: `.AddSingleton<IClock, SystemClock>()` and `.AddSingleton<IHostingService, HostingService>()`
- Create: `tests/DiscordBot.ServicesTests/Services/HostingServiceTests.cs`

**Config types:**

```csharp
public class HostingMessages {
    public List<HostingTier>? Overdue { get; set; }      // null/empty → HostingDefaults.Overdue
    public List<string>? NeverPaid { get; set; }         // null/empty → silent
    public DegradedMode Degraded { get; set; } = new();
}
public class HostingTier { public int MinDays { get; set; } public List<string> Texts { get; set; } = new(); }
public class DegradedMode {
    public bool Enabled { get; set; } = true;
    public int MinDaysOverdue { get; set; } = 90;
    public double FailureChance { get; set; } = 0.2;
    public List<string>? Texts { get; set; }             // null/empty → HostingDefaults.Degraded
}
public static class HostingDefaults { public static readonly List<HostingTier> Overdue = …spec §3.6 texts…; public static readonly List<string> Degraded = …spec §3.7 texts…; }
```

Lists are nullable on purpose: the configuration binder *appends* to pre-populated lists, so defaults must live in `HostingDefaults` and be chosen at read time when the bound list is null or empty.

**Service:**

```csharp
public record HostingStatus(DiscordGuildId GuildId, bool FooterEnabled, HostingPayment? LastPayment,
                            DateOnly? DueOn, int? DaysOverdue, string? FooterText);

public interface IHostingService {
    HostingStatus GetStatus(DiscordGuildId guildId, string? guildName = null);   // never throws
    Result<IReadOnlyList<HostingStatus>> GetOverview(IEnumerable<Guild> guilds);   // one per guild, sorted DaysOverdue desc (nulls last)
    Result RecordPayment(DiscordGuildId guildId, DateOnly paidOn, int termMonths, string? note, DiscordUserId by);
    Result SetFooterEnabled(DiscordGuildId guildId, bool enabled);
    Result SetReminderChannel(DiscordChannelId channel);
    Result<DiscordChannelId?> GetReminderChannel();
    Result MarkUpcomingReminderSent(DiscordGuildId guildId, DateOnly dueOn);
    Result MarkDueReminderSent(DiscordGuildId guildId, DateOnly dueOn);
    bool ShouldDegrade(DiscordGuildId guildId, DiscordUserId userId);   // random draw inside
    string GetDegradedMessage(DiscordGuildId guildId, string? guildName = null);
    IReadOnlyList<GuildHostingState> GetAllStates();                     // for the reminder job
}
```

`HostingService(ILogger<HostingService> logger, IRepositoryStrategy strategy, MessageConfiguration messages, IOptions<BotTeamConfiguration> team, IClock clock, Func<double>? randomSource = null)` — DI resolves the 5 required parameters; `randomSource` defaults to `Random.Shared.NextDouble`. Register with a factory lambda so DI does not choke on the optional parameter: `.AddSingleton<IHostingService>(sp => new HostingService(sp.GetRequiredService<ILogger<HostingService>>(), sp.GetRequiredService<IRepositoryStrategy>(), sp.GetRequiredService<MessageConfiguration>(), sp.GetRequiredService<IOptions<BotTeamConfiguration>>(), sp.GetRequiredService<IClock>()))`.

Core rules (implement exactly):
- Cache: `ConcurrentDictionary<DiscordGuildId, GuildHostingState>` filled lazily from `repo.GetAll()` under a `lock` on first access; every write goes `UpdateOrInsert` then replaces the cache entry.
- `DueOn = DateOnly.FromDateTime(last.PaidOn).AddMonths(last.TermMonths)`; `DaysOverdue = today.DayNumber - DueOn.DayNumber` where `today = DateOnly.FromDateTime(clock.UtcNow)`.
- `FooterText`: null if `!FooterEnabled`; if no payment → random `NeverPaid` text or null when list null/empty; if `DaysOverdue < 0` → null; else pick the tier with the largest `MinDays <= DaysOverdue` from `Overdue ?? HostingDefaults.Overdue`, random text, substitute `{server}` (guildName ?? guildId), `{days}`, `{date}` (`DueOn.ToString("d MMM yyyy")`), `{paidDate}`.
- `ShouldDegrade`: false unless `Degraded.Enabled`, state exists with a payment, `FooterEnabled`, `DaysOverdue >= MinDaysOverdue`, `guildId != team.GuildId`, `userId != team.OwnerId`; then `randomSource() < FailureChance`.
- `RecordPayment`: validate `1 <= termMonths <= 60`, append payment, keep `FooterEnabled`, **do not** touch the reminder-sent dates (they compare against `DueOn`, which changed, so they re-arm naturally).

- [ ] **Tests** (`HostingServiceTests`, `IRepositoryStrategy` via NSubstitute returning in-memory fake repositories, or the real LiteDB fixture; fixed clock `new FakeClock(new DateTime(2026, 9, 12))`; `randomSource` injected):
  - footer: `NoPayment_NoNeverPaidConfig_ReturnsNull`; `NoPayment_WithNeverPaidConfig_ReturnsText`; `Paid_NotYetDue_ReturnsNull`; `DueToday_UsesTier0`; `Overdue7_UsesTier7`; `Overdue29_StillTier7`; `Overdue30_UsesTier30`; `Overdue120_UsesTier90`; `FooterDisabled_ReturnsNull`; `Placeholders_AreSubstituted` (`{server}`, `{days}`, `{date}`); `RandomText_CoversAllTextsOfTier` (seeded/sequence random over 3-text tier hits all 3).
  - dates: `DueOn_Jan31Plus1Month_IsFeb28`; `DaysOverdue_Negative_WhenNotDue`.
  - degrade: `ShouldDegrade_False_BelowThreshold`; `ShouldDegrade_True_WhenDraw0_AtThreshold`; `ShouldDegrade_False_WhenDraw1`; `ShouldDegrade_False_InOwnerGuild`; `ShouldDegrade_False_ForOwner`; `ShouldDegrade_False_WhenFooterDisabled`; `ShouldDegrade_False_WhenDisabledInConfig`.
  - overview: `GetOverview_SortsMostOverdueFirst_UnknownGuildsLast`.
  - write paths: `RecordPayment_AppendsAndUpdatesCache`; `RecordPayment_RejectsTermOutsideRange`; `SetFooterEnabled_CreatesStateForUnknownGuild`; `Mark*ReminderSent_Persists`.
- [ ] Implement, build, tests green, commit `feat(hosting): HostingService with footer, tier, and degrade rules`.

### Task 4: Footer and degraded-mode hooks in `DiscordBot`

**Files:**
- Modify: `DiscordBot/Models/Contexts/BaseInteractiveContext.cs`
- Modify: `DiscordBot/Helpers/Builders/EmbedBuilderHelper.cs` (`WithMessageAuthorFooter`: separator `", "` → `" · "`)
- Modify: `DiscordBot/Services/InteractiveCommandHandlerService.cs` (`OnInteraction`)

`BaseInteractiveContext<T>`:
```csharp
public string? HostingFooter { get; }
// in ctor, after Client is set:
HostingFooter = InGuild ? provider.GetRequiredService<IHostingService>().GetStatus(Guild.GetGuildId(), Guild.Name).FooterText : null;
```
(`Guild.GetGuildId()` is the existing extension in `DiscordBot/Helpers/Extensions/DiscordIdentityHelper.cs`; if `InGuild` throws for DM interactions because `Channel` is not an `IGuildChannel`, guard with `InnerContext.Channel is IGuildChannel`.)
- `CreateEmbedBuilder`: `.WithMessageAuthorFooter(User, HostingFooter ?? string.Empty)`.
- `CreatePageBuilder(string)`: after `.WithCurrentTimestamp()`, `if (HostingFooter is not null) builder.WithFooter(HostingFooter);` (Fergun `PageBuilder.WithFooter(string text, string? iconUrl = null)`; if that overload is absent use `WithFooter(new EmbedFooterBuilder().WithText(HostingFooter))`). Same in the `CreatePageBuilder(EmbedBuilder, string)` overload only when the embed has no footer yet.
- `RespondAsync`/`FollowupAsync`: `if (HostingFooter is not null && (embeds is null || !embeds.Any()) && !string.IsNullOrEmpty(text)) text += "\n-# " + HostingFooter;`

`InteractiveCommandHandlerService.OnInteraction`, after `ctx` is created and logged, before `_commandInstigator.ExecuteCommandAsync`:
```csharp
if (arg is SocketSlashCommand slash && ctx is ApplicationCommandContext appCtx && appCtx.InGuild
    && !string.Equals(appCtx.Command, "hosting", StringComparison.OrdinalIgnoreCase)
    && _hostingService.ShouldDegrade(appCtx.Guild.GetGuildId(), appCtx.User.GetUserId())) {
    var msg = _hostingService.GetDegradedMessage(appCtx.Guild.GetGuildId(), appCtx.Guild.Name);
    _logger.LogInformation("[{ctx}] degraded mode: refusing command", ctx);
    await appCtx.RespondAsync(embeds: new[] { appCtx.CreateEmbedBuilder().WithFailure(msg).Build() });
    return;
}
```
Inject `IHostingService` into the service's constructor.

- [ ] Build the solution (Dashboard too). No unit tests possible here (needs a live `SocketInteraction`); manual check happens in Task 7.
- [ ] Commit `feat(hosting): overdue footer and degraded mode hooks`.

### Task 5: `/hosting` command with guild autocomplete

**Files:** folder `DiscordBot/Commands/Interactive2/Hosting/`
- `HostingRootCommandDefinition.cs` (`Name => "hosting"`, `Description => "Track which servers paid for hosting"`, `ExtendBaseSlashCommandBuilder` → `builder.WithDMPermission(false)`)
- `Paid/PaidSubCommandDefinition.cs` (`Name => "paid"`; options: `ServerOption = "server"` string required `isAutocomplete: true`; `DateOption = "date"` string optional; `MonthsOption = "months"` integer optional with `.WithMinValue(1).WithMaxValue(60)`; `NoteOption = "note"` string optional), `Paid/PaidSubCommandRequest.cs` (`MinimumAuthorizationRole => AuthorizationRoles.BotOwner`), `Paid/PaidSubCommandHandler.cs`
- `Status/StatusSubCommandDefinition.cs` (`server` optional autocomplete), `Status/StatusSubCommandRequest.cs`, `Status/StatusSubCommandHandler.cs`
- `Footer/FooterSubCommandDefinition.cs` (`server` required autocomplete, `enabled` boolean required), request, handler
- `Remind/RemindSubCommandDefinition.cs` (`channel` Channel required), request, handler
- `HostingGuildAutoCompleteRequest.cs`: `AutoCompleteCommandRequestBase<PaidSubCommandDefinition>, IAutoCompleteCommandRequest<StatusSubCommandDefinition>, IAutoCompleteCommandRequest<FooterSubCommandDefinition>`, role `BotOwner`
- `HostingGuildAutoCompleteHandler.cs`: `AutoCompleteHandlerBase<HostingGuildAutoCompleteRequest>`; `DoWork`: `var q = Context.CurrentOptionAsString ?? ""; var options = Context.Client.Guilds.Where(g => g.Name.Contains(q, StringComparison.OrdinalIgnoreCase)).OrderBy(g => g.Name).Take(20).Select(g => (g.Name, g.Id.ToString())); _ = Context.RespondAsync(options); return Result.Ok();`
- `HostingHandlerHelpers.cs` (static): `Result<DiscordGuildId> ResolveServer(string value, DiscordSocketClient client)` (parse `ulong` → id; else exact name match ignoring case; else `Result.Fail("Unknown server")`), `Result EnsureOwnerGuild(ApplicationCommandContext ctx, BotTeamConfiguration team)` (`ctx.InGuild && ctx.Guild.GetGuildId() == team.GuildId` else `Result.Fail("Only available in the bot owner's server")`), `string FormatStatusLine(HostingStatus s, string name)`.

Handler behaviour:
- `paid`: `EnsureOwnerGuild`; resolve server; parse `date` with `DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)` (default today); months default 12; `RecordPayment`; if `GetReminderChannel()` is null → `SetReminderChannel(ctx.Channel id)`; reply ephemeral success embed: `"{server}: paid {date}, {months} months, due {dueOn:d MMM yyyy}"`.
- `status` without server: `_discordService.GetGuilds()` → `GetOverview` → lines `"{name,-24} paid {paidDate,-11} due {dueOn,-11} {daysText,-14} footer {on/off}"` in a code block via `Context.CreatePaginatorReplyBuilder(ephemeral: true).WithLines(lines, 20, header: …, inCodeBlock: true)`; with server: single embed with history (last 10 payments) + current footer text (if any).
- `footer`: `SetFooterEnabled`; ephemeral confirmation.
- `remind`: `SetReminderChannel(channel.ToChannelDto().Id)` (`IChannel` from `GetOptionValue<IChannel>`); ephemeral confirmation.
All handlers get `IHostingService`, `IDiscordService`, `IOptions<BotTeamConfiguration>`, `DiscordSocketClient` through `ServiceProvider.GetRequiredService<…>()` in the constructor, like `ConfigureJobSubCommandHandler`.

- [ ] Build the solution. Run tests (unchanged). Commit `feat(hosting): /hosting command with server autocomplete`.
- [ ] Note for the owner in the final report: run `/commands`, pick `hosting`, register it in the owner guild only.

### Task 6: `SendMentionEmbed` and `HostingReminderJob`

**Files:**
- Modify: `DiscordBot.Services/Interfaces/IDiscordService.cs`, `DiscordBot/Services/DiscordService.cs` — add `Task<Result> SendMentionEmbed(DiscordChannelId channelId, DiscordUserId mention, EmbedBuilder embed)`: `GetChannelAsync` → `ISocketMessageChannel.SendMessageAsync($"<@{mention.UlongValue}>", embed: embed.Build(), allowedMentions: AllowedMentions.All)`; `Result.Fail("Could not send message")` otherwise. (`DiscordBot.Dashboard/Services/CachedDiscordService.cs` implements/decorates `IDiscordService`? grep and add a pass-through if it does.)
- Create: `DiscordBot.Services/Jobs/HostingReminderJob.cs`
- Modify: `DiscordBot.Services/Configuration/QuartzConfiguration.cs` (schedule daily 09:00 in the existing `timeZone`, identity `"hosting-reminder"` group `"hosting"`, `WithMisfireHandlingInstructionFireAndProceed`)
- Create: `tests/DiscordBot.ServicesTests/Jobs/HostingReminderJobTests.cs`

Job (`: BaseJob`, ctor `(ILogger<HostingReminderJob>, IHostingService, IDiscordService, IOptions<BotTeamConfiguration>, IClock)`), `DoWork`:
1. `channel = hosting.GetReminderChannel()`; if null → `Logger.LogWarning("No hosting reminder channel configured")`, return Ok.
2. `guilds = await discord.GetGuilds()`; for each guild with a status that has `DueOn`: decide
   - upcoming: `DaysOverdue is >= -30 and < 0` and `state.UpcomingReminderSentForDueOn != DueOn.ToDateTime(TimeOnly.MinValue)`
   - due: `DaysOverdue >= 0` and `state.DueReminderSentForDueOn != DueOn…`
3. If any: one embed titled `Hosting payments`, one field per guild `"{name}"` → `"due {date} ({N days left|N days overdue}){, note}"`; `SendMentionEmbed(channel, team.OwnerId, embed)`; on success call the matching `Mark…Sent(guildId, dueOn)` per guild; on failure return `Result.Fail` without marking.

Expose the decision as a pure static `HostingReminderJob.Decide(HostingStatus status, GuildHostingState state) : ReminderKind` (`None | Upcoming | Due`) so it is testable without Quartz.

- [ ] **Tests**: `Decide_*` theory over (`DaysOverdue`, sent dates) → kind: `-31 → None`, `-30 not sent → Upcoming`, `-30 already sent for this DueOn → None`, `-1 not sent → Upcoming`, `0 not sent → Due`, `5 due sent → None`, `5 due sent for an older DueOn → Due`. Job test with NSubstitute: two guilds, one due, one fine → `SendMentionEmbed` called once with owner id; `MarkDueReminderSent` called for the due guild only; when `SendMentionEmbed` returns `Fail`, no `Mark…` call.
- [ ] Implement, build, tests green, commit `feat(hosting): owner reminders 30 days before and on the due date`.

### Task 7: Config defaults, docs, manual verification notes

- [ ] Add a `Hosting` block with the default tiers/degraded texts to `DiscordBot/appsettings.default.json` under `Bot:Messages` (mirrors `HostingDefaults`, so operators see the shape). Startup must still work when the block is absent (Task 3 defaults).
- [ ] Update `docs/codebase/02-command-system.md` (new command, autocomplete example, degrade hook), `03-data-layer.md` (two new collections), `04-services-and-jobs.md` (`HostingService`, `HostingReminderJob`, `SendMentionEmbed`), `README.md` index.
- [ ] Set the spec status to `implemented on feature/hosting-tracker-and-litedb-lifecycle`.
- [ ] Write `docs/superpowers/plans/2026-09-12-hosting-manual-checklist.md`: (1) in a dev guild run `ping2 normal` and `count ranking` after `/hosting paid` with a date 13 months ago → footer visible on both; (2) date 4 months ago → roughly 1 in 5 commands returns the degraded embed; (3) `/hosting footer enabled:false` → nothing; (4) temporarily reschedule or trigger `HostingReminderJob` → mention arrives in the reminder channel, second run sends nothing.
- [ ] Commit `docs: hosting tracker`.
