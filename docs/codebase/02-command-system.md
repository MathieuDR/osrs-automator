# Command System (Slash Commands / Interactions)

Scope: `DiscordBot/Commands/Interactive*`, `DiscordBot/Services/*Command*`, `DiscordBot/Models/Contexts/*`,
`DiscordBot/Helpers/Builders/*`, `DiscordBot/Helpers/Extensions/*`. Target: an AI agent adding a **new slash
command safely**, without needing to reverse-engineer the dispatch pipeline again.

---

## 1. Overview: two generations, both live

There are **two parallel command systems in the same process, running at once.** Neither is fully
decommissioned.

- **Legacy ("v1")** — `DiscordBot/Commands/Interactive/*`. One class per top-level command, deriving from
  `ApplicationCommandHandler` (`DiscordBot/Commands/Interactive/ApplicationCommandHandler.cs`), implementing
  `IApplicationCommandHandler`. All handlers are explicitly listed by hand in
  `DiscordBot/Configuration/ConfigurationExtensions.cs:72-83` and wrapped by `CommandStrategy`
  (`DiscordBot/Commands/Interactive/CommandStrategy.cs`). Commands still living here: `ping`, `commands`
  (`ManageCommandsApplicationCommandHandler`), `kill` (`KillBotCommandHandler`), count-config, configure,
  create-competition, authorization-config.
- **Current ("v2"/"Interactive2")** — `DiscordBot/Commands/Interactive2/*`. Reflection-discovered
  Definition/Request/Handler triads dispatched through **MediatR**. This is where all new features go:
  `Ping` (parallel demo of the same command name space), `Graveyard`, `Job`, `Confirm`, `Count`, `CountSelf`,
  `Drops`, `Funds`, `MemberInfo`, `Hosting`.

  `Hosting` (`Commands/Interactive2/Hosting/`) is the owner-only hosting payment tracker
  (`docs/superpowers/specs/2026-09-12-hosting-payment-tracker-design.md`):
  - `HostingRootCommandDefinition` — `/hosting`, `builder.WithDMPermission(false)`.
  - Subcommands, all `SubCommandDefinitionBase<HostingRootCommandDefinition>`:
    - `paid` (`Paid/PaidSubCommandDefinition`) — options `server` (string, required, autocomplete),
      `date` (string, optional, `yyyy-MM-dd`, default today), `months` (integer, optional, min 1 max 60,
      default 12), `note` (string, optional). Records a payment, auto-sets the reminder channel to the
      current channel if none is set yet, replies ephemeral with the new due date.
    - `status` (`Status/StatusSubCommandDefinition`) — `server` (string, optional, autocomplete). Without
      `server`: a paginated code-block overview of every guild the bot is in (via
      `CreatePaginatorReplyBuilder().WithLines(...)`, 20 rows/page), most-overdue first. With `server`:
      that guild's status line plus its last 10 payments.
    - `footer` (`Footer/FooterSubCommandDefinition`) — `server` (autocomplete, required), `enabled`
      (boolean, required). Toggles `GuildHostingState.FooterEnabled` for that guild — also the kill
      switch for degraded mode (§below).
    - `remind` (`Remind/RemindSubCommandDefinition`) — `channel` (channel, required). Sets the owner
      reminder channel via `IHostingService.SetReminderChannel`.
  - **Owner-only gate**: every Request (`PaidSubCommandRequest`, `StatusSubCommandRequest`,
    `FooterSubCommandRequest`, `RemindSubCommandRequest`) declares
    `MinimumAuthorizationRole => AuthorizationRoles.BotOwner` (§4 — de-facto single-user gate).
  - **Owner-guild guard**: every Handler's `DoWork` calls
    `HostingHandlerHelpers.EnsureOwnerGuild(Context, botTeamConfiguration)` first
    (`Commands/Interactive2/Hosting/HostingHandlerHelpers.cs`), which fails with
    `"Only available in the bot owner's server"` unless the interaction is in a guild channel whose id
    matches `Bot:TeamConfiguration:GuildId`.
  - **Registration**: like every Interactive2 command, `/hosting` is invisible in Discord until an
    owner/admin runs the legacy `/commands` command once, picks `hosting`, and registers it to the owner
    guild (§3) — it is not force-registered like `commands`/`kill`.

**Both are wired up simultaneously and cooperate via fallback:**
- `InteractiveCommandHandlerService.OnInteraction` (`DiscordBot/Services/InteractiveCommandHandlerService.cs:64-120`)
  first tries the new system (`ICommandInstigator.ExecuteCommandAsync`). If that fails with a `"404"` error
  metadata (command/subcommand not found in Interactive2), it falls back to the legacy
  `ICommandStrategy.HandleInteractiveCommand(ctx)` (line 99).
- Registration is layered the same way via the **decorator pattern** (Scrutor `.Decorate`, see §3):
  `CommandDefinitionRegistrationService` (new) wraps `CommandRegistrationService` (old) — if a command name
  isn't found among the new Definitions it delegates to the old registration path
  (`DiscordBot/Services/CommandDefinitionRegistrationService.cs:49-55`).

**Conclusion for an agent adding a command:** always add it to **Interactive2**. Never add a new class under
`Commands/Interactive/`.

---

## 2. Interactive2 anatomy

Three files per command (per root, per subcommand, per autocomplete field, per button):
**Definition** (schema), **Request** (DTO + MediatR envelope), **Handler** (the actual work).

### Definitions — `Commands/Interactive2/Base/Definitions/`

- `ICommandDefinition` — `Name`, `Description`, `Options` (name/type tuples).
- `IRootCommandDefinition : ICommandDefinition` — adds `GetCommandBuilderHash()` (an `XXHash32` of the
  serialized `SlashCommandProperties`, used to detect schema changes — see §3) and `GetCommandProperties()`.
- `ISubCommandDefinition : ICommandDefinition` — adds `GetOptionBuilder() : Task<SlashCommandOptionBuilder>`.
- `ISubCommandDefinition<TParentCommand> : ISubCommandDefinition where TParentCommand : IRootCommandDefinition`
  — the generic parameter is how a subcommand declares which root it belongs to; this is also how reflection
  groups sub-under-root in `CommandDefinitionProvider` (§3).
- `RootCommandDefinitionBase` (`.../Base/Definitions/RootCommandDefinitionBase.cs`) — abstract base for roots.
  Builds a `SlashCommandBuilder`, calls the overridable `ExtendBaseSlashCommandBuilder(builder)` hook (add
  root-level options here, if any), then auto-attaches every injected `ISubCommandDefinition`'s option builder
  via `AddSubCommands`. Validates names are lowercase.
- `SubCommandDefinitionBase<TRoot>` (`.../Base/Definitions/SubCommandDefinitionBase.cs`) — abstract base for
  subcommands. Builds a `SlashCommandOptionBuilder` of type `SubCommand`, calls the abstract
  `ExtendOptionCommandBuilder(builder)` hook where you add the subcommand's actual options
  (`builder.AddOption(name, ApplicationCommandOptionType.X, description, required, isAutocomplete:)`).

### Requests — `Commands/Interactive2/Base/Requests/`

- `ICommandRequest<out TContext> : IRequest<Result>` (MediatR) — carries `MinimumAuthorizationRole`
  (`AuthorizationRoles`) and `Context` (a `BaseInteractiveContext`).
- `ICommandRequest<out TCommandDefinition, out TContext> : ICommandRequest<TContext>` — the
  `TCommandDefinition` generic parameter is the load-bearing bit: it's how `CommandInstigator` maps an
  incoming interaction (which resolves to a concrete `ICommandDefinition` instance) to the *Request type* that
  should be constructed and sent through MediatR (see §2 sequence, step 5).
- Three flavors, one per interaction kind, all `where TCommandDefinition : ICommandDefinition`:
  - `IApplicationCommandRequest<TCommandDefinition> : ICommandRequest<TCommandDefinition, ApplicationCommandContext>`
  - `IAutoCompleteCommandRequest<TCommandDefinition> : ICommandRequest<TCommandDefinition, AutocompleteCommandContext>`
  - `IMessageComponentCommandRequest<TCommandDefinition> : ICommandRequest<TCommandDefinition, MessageComponentContext>`
- Concrete bases you actually inherit from: `ApplicationCommandRequestBase<TCommandDefinition>`,
  `AutoCompleteCommandRequestBase<TCommandDefinition>`, `MessageComponentRequestBase<TCommandDefinition>` — all
  take the context in the constructor and require you to override `MinimumAuthorizationRole`.
- **A single Request class can implement `ICommandRequest<TDefinition, TContext>` for *multiple* definitions.**
  Example: `ShameLocationAutoCompleteRequest` (`Graveyard/ShameLocationAutoCompleteRequest.cs`) implements
  `AutoCompleteCommandRequestBase<ShameSubCommandDefinition>` **and**
  `IAutoCompleteCommandRequest<ShamesSubCommandDefinition>`, `IAutoCompleteCommandRequest<LeaderboardSubCommandDefinition>`,
  `IAutoCompleteCommandRequest<EditShameSubcommandDefinition>` — one handler backs the same autocompleted
  "location" option across four different subcommands.

### Handlers — `Commands/Interactive2/Base/Handlers/`

- `ICommandHandler<in TRequest, TContext> : IRequestHandler<TRequest, Result>` (MediatR).
- `CommandHandlerBase<TRequest, TContext>` (`.../Base/Handlers/CommandHandlerBase.cs`) — the real engine.
  `Handle(request, ct)` sets `Context`, `Request`, `CommandDefinition` (see gotcha in §8 — this is
  re-instantiated via `Activator.CreateInstance`, **not** the singleton instance from the provider), then calls
  the abstract `DoWork(ct) : Task<Result>` you implement.
- Three thin subclasses fix the `TContext` type parameter: `ApplicationCommandHandlerBase<TRequest>` (Context =
  `ApplicationCommandContext`), `AutoCompleteHandlerBase<TRequest>` (Context = `AutocompleteCommandContext`),
  `MessageComponentHandlerBase<TRequest>` (Context = `MessageComponentContext`).
- Replies happen inside `DoWork` via `Context.RespondAsync(...)`, `Context.CreateReplyBuilder()...RespondAsync()`,
  `Context.DeferAsync()` + later `Context.FollowupAsync(...)`, or `Context.CreatePaginatorReplyBuilder()` /
  `Context.SendPaginator(...)` for paged output (Fergun.Interactive). Return `Result.Ok()` / `Result.Fail(...)`.

### Sequence: Discord interaction → reply

1. Discord fires a gateway event → Discord.Net's `DiscordSocketClient.InteractionCreated`, subscribed once in
   `InteractiveCommandHandlerService`'s constructor: `client.InteractionCreated += OnInteraction;`
   (`DiscordBot/Services/InteractiveCommandHandlerService.cs:41`).
2. `OnInteraction(SocketInteraction arg)` (lines 64-120) pattern-matches the concrete interaction type and
   wraps it in the matching context: `SocketSlashCommand` → `ApplicationCommandContext`,
   `SocketMessageComponent` → `MessageComponentContext` (unless the message belongs to a Fergun
   `InteractiveService` paginator callback, in which case it's ignored — line 73-77),
   `SocketAutocompleteInteraction` → `AutocompleteCommandContext`.
3. `await _commandInstigator.ExecuteCommandAsync(ctx)` — `CommandInstigator.ExecuteCommandAsync(BaseInteractiveContext)`
   (`DiscordBot/Services/CommandInstigator.cs:98-105`) dispatches to the generic overload.
4. `GetCommandDefinition(context)` (lines 121-141): looks up the root `ICommandDefinition` whose `Name`
   matches `context.Command` (the slash command name, or for buttons the first `.`-segment before `:` in the
   custom id — see §5) in the dictionary built once at startup from `ICommandDefinitionProvider`. If
   `context.SubCommand` is set, narrows to the matching `ISubCommandDefinition` by `Name`. Not found → `Result.Fail`
   tagged with metadata key `"404"`.
5. `CreateCommandRequest(context, definition)` (lines 107-119): looks up a pre-compiled constructor delegate
   (`TypeHelper.ObjectActivator`) keyed by `(definition, context.GetType())` in `_commandRequestsActivators` —
   built once in the constructor (`InitializeCommandRequestDictionary`, lines 35-72) by reflecting over every
   `Type` implementing `ICommandRequest<>` in the assembly and matching its generic arguments to the
   definition type and context type. Invokes it → a concrete Request instance.
6. `_commandAuthorizationService.IsAuthorized(request, context)` — see §4. Not authorized → `Result.Fail`
   with metadata key `"501"`.
7. `_mediator.Send(request)` — MediatR resolves the matching `IRequestHandler<TRequest, Result>` (your
   Handler) from DI and calls `Handle`.
8. Handler's `DoWork` runs, replies via the `Context`, returns `Result`.
9. Back in `OnInteraction`: if the result failed with `"404"`, retries via legacy `ICommandStrategy.HandleInteractiveCommand(ctx)`
   (line 99). If still failed for any reason, builds a failure embed and sends it via
   `arg.FollowupAsync(..., ephemeral: true)` if the context was deferred, else `arg.RespondAsync(..., ephemeral: true)`
   (lines 102-115).

---

## 3. Registration: Definition → Discord application command

### Discovery (reflection, not manual DI registration)

- `CommandDefinitionProvider` (`DiscordBot/Services/CommandDefinitionProvider.cs`) is constructed with
  `Type[] assemblyTypes` (in practice just `typeof(Program)`, passed via `AddDiscordBot<Program>(config)` in
  `DiscordBot/Program.cs:50`). Its constructor calls
  `assemblyTypes.GetConcreteClassFromType(typeof(ICommandDefinition))`
  (`DiscordBot/Helpers/Extensions/TypeHelper.cs:126-138`), which scans **every type in the `DiscordBot`
  assembly** for non-abstract classes assignable to `ICommandDefinition`.
- `SortCommandDefinitionTypesByRootCommand` splits these into roots (`IRootCommandDefinition`) and subs
  (`ISubCommandDefinition<>`), grouping each sub under the root type that appears in its generic argument list.
- `ActivateTypes` then `Activator.CreateInstance`s all of them (subs first, injecting `IServiceProvider`; then
  the root, injecting the sub instances as `IEnumerable<ISubCommandDefinition>`).
- **You do not register a new command anywhere.** Adding a `RootCommandDefinitionBase`/`SubCommandDefinitionBase<T>`
  class to the `DiscordBot` assembly is sufficient for it to be discovered on next process start.
- Requests (§2) and Handlers are similarly auto-discovered: Requests via the same
  `GetConcreteClassFromType(typeof(ICommandRequest<>))` scan (`ConfigurationExtensions.cs:126`), and Handlers
  automatically by **MediatR's own assembly scan** —
  `.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly))`
  (`DiscordBot/Configuration/ConfigurationExtensions.cs:50`) registers every `IRequestHandler<,>` implementation
  in DI, so a new Handler class needs **no explicit `AddScoped`/`AddSingleton` line**.

### Turning a Definition into a live Discord command

This is the part that **is not automatic on every startup** — it is gated by a persisted
`ApplicationCommandInfo` record.

- `ApplicationCommandInfo` (`DiscordBot.Common/Models/Data/Configuration/ApplicationCommandInfo.cs`): `CommandName`,
  `Hash` (uint, from `GetCommandBuilderHash()`), `IsGlobal` (bool), `RegisteredGuilds` (`List<DiscordGuildId>`).
  Persisted via `IApplicationCommandInfoRepository` (LiteDB-backed, `DiscordBot.Data`).
- At startup, `InteractiveCommandHandlerService.SetupAsync()` waits for the client to connect
  (`ClientOnConnected`) then calls `Initialize() → InitializeCommands()`
  (`DiscordBot/Services/InteractiveCommandHandlerService.cs:46-62, 122-131`):
  1. Force-(re)registers the legacy `commands` (`ManageCommandsApplicationCommandHandler`) and `kill`
     (`KillBotCommandHandler`) handlers **directly into the bot-team owner guild only**
     (`RegisterCommandForOwnersGuild`, lines 133-149) — deletes any existing guild command of that name and
     recreates it via `_client.Rest.CreateGuildCommand(...)`.
  2. `var commandInfos = _commandInfoRepository.GetAll().Value;` then
     `await _registrationService.UpdateAllCommands(commandInfos);` — **this only iterates over
     `ApplicationCommandInfo` rows that already exist in the database.** A command with no persisted row is
     never touched here.
- `CommandDefinitionRegistrationService.UpdateCommand(ApplicationCommandInfo info)`
  (`DiscordBot/Services/CommandDefinitionRegistrationService.cs:49-55, 25-47`): finds the matching root
  Definition by `info.CommandName`; if none found, delegates to the wrapped legacy
  `ICommandRegistrationService` (`_oldRegistration`, decorated in via Scrutor). Otherwise:
  1. Recomputes `currentHash` from the live Definition (`GetCommandBuilderHash()`).
  2. `HandleGlobalRegistration`: if the hash changed, deletes the existing global command (if any); if
     `info.IsGlobal` and no global command exists, creates one via `_client.CreateGlobalApplicationCommandAsync`.
  3. `HandleGuildRegistries`: for every guild the client is in, deletes the guild-scoped command if the hash
     changed; (re)creates it via `guild.CreateApplicationCommandAsync` **only if** `info.RegisteredGuilds`
     contains that guild's id; deletes it if the guild id was removed from the list.
  4. If the hash changed, updates the persisted `ApplicationCommandInfo.Hash`.
- **How an `ApplicationCommandInfo` row is created for a brand-new command**: via the legacy `/commands`
  slash command (`ManageCommandsApplicationCommandHandler`, `MinimumAuthorizationRole = BotAdmin`). Its
  `GetCommandsSelectMenu()` lists both legacy (`ICommandStrategy.GetCommandDescriptions()`) **and** Interactive2
  (`ICommandDefinitionProvider.GetRootDefinitionDescriptions()`) root commands. Picking a command then a guild
  (`HandleGuildSubCommand`) or "Register globally" (`HandleGlobalSubCommand`) creates the
  `ApplicationCommandInfo` if missing (computing its initial hash from the Definition), calls
  `ICommandRegistrationService.UpdateCommand(info)` to push it to Discord immediately, and
  `_applicationCommandInfoRepository.UpdateOrInsert(info)` to persist it.

### Step by step: making a brand-new command appear in Discord

1. Add the Definition/Request/Handler classes under `Commands/Interactive2/<Feature>/` (§7 recipe).
2. Rebuild and **restart the bot process** — discovery happens in `CommandDefinitionProvider`'s constructor
   and MediatR's assembly scan, both at DI-container build time.
3. As a user with `BotAdmin` (or the configured owner), run `/commands`, pick the new command from the select
   menu, and either click **"Register globally"** or pick a guild from the guild select menu. This is what
   actually calls Discord's create-command REST endpoint and persists the `ApplicationCommandInfo` row.
4. On every subsequent restart, `InitializeCommands()` re-syncs idempotently by hash comparison — you do not
   need to re-run `/commands` unless you want to change scope (global vs. specific guilds) or the schema
   changed (name/description/options — that changes the hash and forces delete+recreate automatically).

---

## 4. Authorization

- `AuthorizationRoles` (`DiscordBot.Common/Models/Enums/AuthorizationRoles.cs`) is a `[Flags]` enum, **lower
  numeric value = more privileged**: `BotOwner=1, BotAdmin=2, BotModerator=4, ClanOwner=8, ClanAdmin=16,
  ClanModerator=32, ClanEventHost=64, ClanEventParticipant=128, ClanMember=256, ClanGuest=512, None=1024`.
- Every Request overrides `MinimumAuthorizationRole`. `None` means "everyone" (checked first, short-circuits).
- Enforcement: `CommandAuthorizationService.CheckAuthorization`
  (`DiscordBot/Services/CommandAuthorizationService.cs:36-100`), called from `CommandInstigator` before
  `_mediator.Send`. Order of checks:
  1. `roleRequired == AuthorizationRoles.None` → allow.
  2. `context.User.GetUserId() == OwnerId` (from `BotTeamConfiguration.OwnerId`, config-bound
     `Bot:TeamConfiguration:OwnerId`) → **always allow, bypasses everything else.**
  3. **`if (roleRequired <= AuthorizationRoles.BotModerator) return false;`** (line 50-53) — i.e. if the
     command demands `BotOwner`, `BotAdmin`, or `BotModerator`, and the caller isn't the configured owner
     (step 2), **it is always denied, full stop** — no guild role, server-owner, or per-user config can grant
     it. This is the de-facto "developer/owner only" gate.
  4. Otherwise (guild-level roles: `ClanOwner` and below): requires `context.InGuild`; then checks
     server-owner (`context.Guild.OwnerId == context.User.Id`, only satisfies `ClanOwner`-level minimums or
     weaker), then per-guild `CommandRoleConfig.UserIds`/`RoleIds` overrides (fetched via `IGroupService`,
     cached 3h in `GuildConfigs`), then the caller's actual Discord role IDs.
- **Concrete owner-only example**: `KillBotCommandHandler` (legacy system,
  `DiscordBot/Commands/Interactive/KillBotCommandHandler.cs`) declares
  `public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotAdmin;` and
  `GlobalRegister => false`. Combined with the check above, this means **only the single Discord user id in
  `BotTeamConfiguration.OwnerId` can ever run `/kill`** — and it is additionally only ever registered as a
  guild command in the owner's own guild (`RegisterCommandForOwnersGuild`, §3), never made public. It calls
  `Environment.Exit(0)` after disposing the LiteDB manager. `ManageCommandsApplicationCommandHandler` (`/commands`)
  uses the same pattern at `BotAdmin` level, so it is likewise owner-only.
- `CommandRoleConfig` (`DiscordBot.Common/Models/Data/Configuration/CommandRoleConfig.cs`): per-guild
  `Dictionary<DiscordRoleId, AuthorizationRoles> RoleIds` and `Dictionary<DiscordUserId, AuthorizationRoles> UserIds`,
  edited via `IAuthorizationService`/`AuthorizationService` (`DiscordBot.Services/Services/AuthorizationService.cs`).
  `BotPermissions` (`DiscordBot.Common/Models/Enums/BotPermissions.cs`, `[Flags] None|EventManager|CompetitionManager`)
  is a separate, narrower flag set used elsewhere for feature gating (e.g. competitions) — not consulted by
  `CommandAuthorizationService`.

---

## 5. Autocomplete and message-component (button) routing

**Autocomplete** goes through the identical `CommandInstigator` pipeline with an `AutocompleteCommandContext`.
`context.Command`/`context.SubCommand` are read the same way as `ApplicationCommandContext` (from
`InnerContext.Data.CommandName` / the `SubCommand`-typed option). Your Definition marks an option
`isAutocomplete: true` (e.g. `Graveyard/ShameSubCommand/ShameSubCommandDefinition.cs:14`:
`builder.AddOption(LocationOption, ApplicationCommandOptionType.String, "...", true, isAutocomplete: true)`),
and a Handler derives `AutoCompleteHandlerBase<TRequest>`, reading `Context.CurrentOptionAsString` /
`Context.Current` and replying via `Context.RespondAsync(IEnumerable<string>)` or
`Context.RespondAsync(IEnumerable<(string name, T value)>)` (capped to 20 results — Discord's limit — inside
`AutocompleteCommandContext.RespondAsync`, `DiscordBot/Models/Contexts/AutocompleteCommandContext.cs:25-31`).
One Request/Handler pair can serve the same option name across several subcommands by implementing
`IAutoCompleteCommandRequest<T>` multiple times (see §2, `ShameLocationAutoCompleteRequest`).

A second example of the same one-handler-many-subcommands pattern:
`HostingGuildAutoCompleteHandler`/`HostingGuildAutoCompleteRequest`
(`Commands/Interactive2/Hosting/HostingGuildAutoCompleteHandler.cs`) backs the `server` option across
`paid`, `status`, and `footer` — `HostingGuildAutoCompleteRequest` derives
`AutoCompleteCommandRequestBase<PaidSubCommandDefinition>` and additionally implements
`IAutoCompleteCommandRequest<StatusSubCommandDefinition>` and
`IAutoCompleteCommandRequest<FooterSubCommandDefinition>`. Discord has no "guild" option type, so `server`
is a plain string option (`isAutocomplete: true`); the handler reads `Context.Client.Guilds`, filters by
`Context.CurrentOptionAsString` (case-insensitive `Contains` on the guild name), takes 20, and responds
with `(name, id-as-string)` pairs — the **label is the guild name, the value is the guild id as a
string**. `HostingHandlerHelpers.ResolveServer` (used by every `Hosting` handler, not just autocomplete)
then parses that value back: `ulong.TryParse` first, else an exact case-insensitive name match against
`DiscordSocketClient.Guilds`, else `Result.Fail("Unknown server")` — covering both a picked-from-list
value and free-typed text.

**Message components (buttons/select menus)**, Interactive2 custom-id format
(`DiscordBot/Models/Contexts/MessageComponentContext.cs:10-13`):

```
CustomId := "<command>[.<subcommand>...]:<param1>[.<param2>...]"
CustomIdParts = CustomId.Split(':').First().Split('.')   // → Command / SubCommand lookup
Parameters   = CustomId.Split(':').Last().Split('.')     // → arbitrary handler-defined args
Command    = CustomIdParts[0]
SubCommand = CustomIdParts.Length > 1 ? CustomIdParts[1] : null
```

Concrete example: `DiscordBot/Services/DiscordService.cs:122-124` builds buttons with custom ids
`"confirm:confirmed"` and `"confirm:declined"` — `Command = "confirm"` (matches `ConfirmRootCommandDefinition.Name`),
no subcommand, `Parameters[0]` is `"confirmed"`/`"declined"`. These are routed to
`ConfirmButtonHandler : MessageComponentHandlerBase<ConfirmButtonRequest>`
(`Commands/Interactive2/Confirm/ConfirmButtonHandler.cs`), which reads `Context.Parameters[0]`.

The **legacy system uses a different, dot-only format** (no `:`):
`ApplicationCommandHandler.SubCommand(params string[] ids)` (`Commands/Interactive/ApplicationCommandHandler.cs:65-71`)
joins `Name + "." + ids` (e.g. `"commands.cancel"`, `"commands.command"`), and
`ApplicationCommandHandler.CanHandle(MessageComponentContext)` compares `CustomIdParts.FirstOrDefault()` to `Name`.
Don't mix the two formats when extending either system.

`MessageComponentContext` also exposes `SelectedMenuOptions` (select-menu values), `EmbedFields` (fields of the
message being interacted with — used by legacy handlers to carry state across button clicks, e.g.
`ManageCommandsApplicationCommandHandler.HandleGuildSubCommand` reads the "Command" field back out of the
embed), and `UpdateAsync(...)` to edit the original message in place.

---

## 5a. Reply-pipeline hooks: hosting footer and degraded mode

Two cross-cutting hooks from the hosting payment tracker
(`docs/superpowers/specs/2026-09-12-hosting-payment-tracker-design.md`) sit in the shared pipeline rather
than in the `Hosting` command itself, so they apply to every command.

**Footer hook** — `BaseInteractiveContext<T>` (`DiscordBot/Models/Contexts/BaseInteractiveContext.cs`):
the constructor resolves `IHostingService.GetStatus(guild.Id, guild.Name).FooterText` once per interaction
(only `InGuild`; `null` in DMs and on any resolution failure — wrapped in try/catch so a broken common DB
never breaks command dispatch) and stores it as `HostingFooter`. It is then surfaced three ways:
1. `CreateEmbedBuilder(title, content)` passes `HostingFooter ?? string.Empty` as the `appendToFooter`
   argument of `WithMessageAuthorFooter` (`EmbedBuilderHelper.cs`), which joins it onto the "Requested by
   …" footer with `" · "` as separator when non-empty.
2. `CreatePageBuilder(...)` (both overloads) does **not** call `.WithFooter(HostingFooter)`: Fergun's
   `StaticPaginatorBuilder.WithFooter(PaginatorFooter.Users | PaginatorFooter.PageNumber)`, set once in
   `GetBaseStaticPaginatorBuilder` below, overrides any footer an individual page sets when it renders the
   paginator, so a `.WithFooter` call on a `PageBuilder` never actually shows. Instead, when `HostingFooter`
   is non-null, it is appended as Discord subtext to the end of the page **description** (`description +
   "\n-# " + HostingFooter`) — this keeps Fergun's own users/page-number footer intact while still
   surfacing the hosting line at the bottom of every page's body. The same subtext-in-description approach
   is used by every page-producing path in `InteractionPaginatorReplyBuilder`
   (`DiscordBot/Helpers/Builders/InteractionPaginatorReplyBuilder.cs`) — `CreatePagesFromLines`, which both
   `WithLines` and `WithLeaderboard` build on — where the subtext's length is reserved in the page-length
   accounting exactly like the existing `footer` parameter, so a page can never exceed Discord's 4096-char
   description limit; when a caller also passes a `footer`, the hosting line goes after it.
3. `RespondAsync`/`FollowupAsync` overrides: `AppendHostingFooter` appends `"\n-# " + HostingFooter`
   (Discord subtext markdown) to the reply text when there are no embeds and the text is non-empty —
   covers text-only replies (e.g. `ping2 normal`).

**Degraded-mode check** — `InteractiveCommandHandlerService.OnInteraction`
(`DiscordBot/Services/InteractiveCommandHandlerService.cs`), right after the context is built and before
`_commandInstigator.ExecuteCommandAsync`: for a `SocketSlashCommand` in a guild whose command name is not
`"hosting"`, it calls `_hostingService.ShouldDegrade(guild.Id, user.Id)`; if true, it responds immediately
with a failure embed built from `_hostingService.GetDegradedMessage(...)` and returns — the command never
runs. `ShouldDegrade` itself (in `HostingService`) is what excludes the owner guild, the owner user, and a
guild with `FooterEnabled == false`, plus the "not overdue enough"/"degraded mode disabled" cases — the
`OnInteraction` check only excludes autocomplete/button interactions (pattern-matched on
`SocketSlashCommand`) and the `hosting` command name itself.

**Config layering gotcha for `Bot:Messages:Hosting:Overdue`:** the .NET configuration binder merges JSON
array elements by index, not by replacing the whole array, so an environment-specific `appsettings.*.json`
that overrides `Overdue` with fewer tier entries than the base `appsettings.json` still ends up with the
base file's extra trailing tiers merged in — to fully replace the tier list, override every index (or keep
`MinDays` consistent across files so the merge lands on the intended tier).

---

## 6. Posting to an arbitrary channel (not just replying to the interaction)

`Context.RespondAsync`/`FollowupAsync`/`CreateReplyBuilder()` only reply to the channel/interaction that
triggered the command. To post into a **different, configured channel** (what scheduled Jobs do, and what
`Job/Configure` sets up), inject `IDiscordService` (`DiscordBot.Services/Interfaces/IDiscordService.cs`,
impl `DiscordBot/Services/DiscordService.cs`) instead of using `Context`:

- `SendSuccessEmbed(DiscordChannelId, string)` / `SendFailedEmbed(DiscordChannelId, string, Guid)` /
  `SendWomGroupSuccessEmbed(...)` / `SendConfirmationMessage(DiscordChannelId, title, description, fields, thumbnailUrl)`
  / `MessageLeaderboards(...)` / `TrackClanFundEvent(...)` / `PrintRunescapeDataDrop(RunescapeDropData, DiscordGuildId, DiscordChannelId)`.
- Underlying Discord.Net calls, by channel/guild id rather than context (`DiscordService.cs`):
  - `_client.GetChannelAsync(channelId.UlongValue)` then cast to `ISocketMessageChannel` and call
    `.SendMessageAsync(...)` (private `SendEmbed` helper, lines 166-175).
  - `_client.GetGuild(guildId.UlongValue).GetTextChannel(channelId.UlongValue)` then
    `.SendMessageAsync(...)` / `.SendFileAsync(stream, filename, caption)` (`PrintRunescapeDataDrop`, lines 36-54;
    `TrackClanFundEvent`, lines ~197-198).
- `ConfigureJobSubCommandHandler` (`Commands/Interactive2/Job/Configure/ConfigureJobSubCommandHandler.cs`)
  is the pattern for **letting a command configure which channel a background job posts into**: it takes a
  `Channel`-typed option, converts via `channel.ToChannelDto()`, and stores it via
  `IGroupService.SetAutomationJobChannel(jobType, user, channelDto, enabled)`. The Job itself
  (`DiscordBot.Services/Jobs/*`) later reads that stored channel id and calls `IDiscordService` to post there —
  it does not go through any `Context` at all, since jobs run on a Quartz schedule, not from an interaction.

---

## 7. Recipe: adding a new root command with one subcommand

Convention split observed in the codebase: `Ping/` keeps all its subcommand files flat in one folder
(`Commands/Interactive2/Ping/*.cs`, all namespace `DiscordBot.Commands.Interactive2.Ping`). Every other feature
(`Graveyard`, `Job`, `Confirm`, `Funds`, `Count`, `CountSelf`) gives **each subcommand its own subfolder**
(`Job/Queue/`, `Job/Configure/`) with its own namespace segment. Prefer the subfolder-per-subcommand style for
anything beyond a single trivial subcommand; it's the majority convention. Below follows that style, using
Ping's exact code shape as the literal template (per the brief) for a feature called `Example` with one
subcommand `Run`.

Files to create:

```
DiscordBot/Commands/Interactive2/Example/ExampleRootCommandDefinition.cs
DiscordBot/Commands/Interactive2/Example/Run/RunExampleSubCommandDefinition.cs
DiscordBot/Commands/Interactive2/Example/Run/RunExampleSubCommandRequest.cs
DiscordBot/Commands/Interactive2/Example/Run/RunExampleSubCommandHandler.cs
```

`ExampleRootCommandDefinition.cs` (mirrors `Ping/PingRootCommandDefinition.cs`):

```csharp
namespace DiscordBot.Commands.Interactive2.Example;

public class ExampleRootCommandDefinition : RootCommandDefinitionBase {
    public override string Name => "example";
    public override string Description => "Example command";

    protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) {
        return Task.FromResult(builder);
    }

    public ExampleRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions)
        : base(serviceProvider, subCommandDefinitions) { }
}
```

`Run/RunExampleSubCommandDefinition.cs` (mirrors `Ping/InsultSubCommandDefinition.cs`):

```csharp
namespace DiscordBot.Commands.Interactive2.Example.Run;

public class RunExampleSubCommandDefinition : SubCommandDefinitionBase<ExampleRootCommandDefinition> {
    public RunExampleSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }

    public override string Name => "run";
    public override string Description => "Run the example";

    public static string TargetOption => "target";

    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(TargetOption, ApplicationCommandOptionType.String, "What to run against", true);
        return Task.FromResult(builder);
    }
}
```

`Run/RunExampleSubCommandRequest.cs` (mirrors `Ping/InsultCommandRequest.cs`):

```csharp
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Example.Run;

public class RunExampleSubCommandRequest : ApplicationCommandRequestBase<RunExampleSubCommandDefinition> {
    public RunExampleSubCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}
```

`Run/RunExampleSubCommandHandler.cs` (mirrors `Ping/InsultApplicationCommandHandler.cs`, plus reading an option
like `Graveyard/ShameSubCommand/ShameCommandHandler.cs` does):

```csharp
namespace DiscordBot.Commands.Interactive2.Example.Run;

public class RunExampleSubCommandHandler : ApplicationCommandHandlerBase<RunExampleSubCommandRequest> {
    public RunExampleSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
        var target = Context.SubCommandOptions.GetOptionValue<string>(RunExampleSubCommandDefinition.TargetOption);

        await Context.CreateReplyBuilder()
            .WithEmbed(e => e.WithSuccess($"Ran against {target}"))
            .RespondAsync();

        return Result.Ok();
    }
}
```

Checklist:

- [ ] Root Definition class name ends in `RootCommandDefinition` (or `RootDefinition`), derives
      `RootCommandDefinitionBase`, lowercase `Name`.
- [ ] Subcommand Definition derives `SubCommandDefinitionBase<TRoot>` where `TRoot` is your root Definition type.
- [ ] Request derives the matching `*RequestBase<TSubCommandDefinition>` for the interaction kind
      (`ApplicationCommandRequestBase` for slash commands) and **must** override `MinimumAuthorizationRole`.
- [ ] Handler derives the matching `*HandlerBase<TRequest>`, implements `DoWork`, replies via `Context`, returns
      `Result.Ok()`/`Result.Fail(...)`.
- [ ] No DI registration needed anywhere — reflection + MediatR pick it up automatically (§3).
- [ ] Restart the bot, then run `/commands` (as `BotAdmin`/owner) to actually push it to Discord (§3) — a
      code-only change is invisible in Discord until this step.
- [ ] If you need a button, add a `MessageComponentRequestBase<T>`/`MessageComponentHandlerBase<T>` pair and
      build custom ids as `"<Name>:<param>.<param>"` (§5) — do **not** reuse the legacy dot-joined format.
- [ ] If you need autocomplete, mark the option `isAutocomplete: true` in the Definition and add an
      `AutoCompleteCommandRequestBase<T>`/`AutoCompleteHandlerBase<T>` pair (§5).

---

## 8. Gotchas for an AI editing this code

- **Two systems, one process.** Don't "helpfully" migrate a legacy command while doing something unrelated —
  it changes registration/authorization code paths (`ICommandStrategy`, the decorator chain) that other legacy
  commands still depend on.
- **New commands are invisible in Discord until someone runs `/commands`.** Writing correct Definition/Request/
  Handler code is necessary but not sufficient — don't tell a user "it's done" without either running `/commands`
  yourself (if you have bot access) or telling them to.
- **The command name is derived from `Name`, which must be lowercase** (`RootCommandDefinitionBase.ValidateCommandBuilder`
  throws `ArgumentException` otherwise, at hash/build time — i.e. at next interaction/registration, not compile time).
- **Hash-based change detection means editing a Definition's options/description is enough to trigger an
  automatic delete+recreate on next `UpdateAllCommands`** — but only for commands that already have an
  `ApplicationCommandInfo` row (§3). A schema change to a command that was never registered via `/commands`
  does nothing.
- **`CommandHandlerBase.GetCommandDefinition()` creates a *new*, throwaway instance of the Definition via
  reflection** (`CommandHandlerBase.cs:27-46`) purely so the handler has a `CommandDefinition` object to look
  at — it is **not** the singleton instance held by `CommandDefinitionProvider`. For a root definition it's
  constructed with an **empty** `IEnumerable<ISubCommandDefinition>` (line 39) — calling
  `GetCommandBuilderHash()`/`GetCommandProperties()` on `CommandDefinition` from inside a handler will silently
  omit all subcommands. Don't rely on it for anything beyond `Name`/`Description`/`Options`.
- **`ICommandRequest<T>` can be implemented multiple times by one class** to share an autocomplete
  handler across several subcommands (§2) — when searching for "the" handler for a given Definition, check for
  this before assuming one-to-one.
- **`CommandAuthorizationService` role comparison is numeric, not semantic** — `AuthorizationRoles` is a
  `[Flags]` enum but treated with `<=`/`<` as an ordered scale (lower = more powerful), not bitwise-ORed. Don't
  combine roles expecting flag semantics in `MinimumAuthorizationRole`.
- **Any `MinimumAuthorizationRole` of `BotOwner`, `BotAdmin`, or `BotModerator` is bot-owner-only** in practice
  (§4) — there is no way to grant a second person `BotAdmin`-or-above access short of changing
  `BotTeamConfiguration.OwnerId` in config. If a task asks for a "trusted helper" role above server owner, it
  cannot be expressed today without new authorization logic.
- **Legacy and new custom-id formats differ** (`:` + `.` vs. pure `.`) — see §5. Mixing them breaks routing
  silently (`CanHandle`/`CustomIdParts` parsing just won't match, no exception).
- **`DiscordBot/Documentation/CommandStrategy.puml` is empty** — do not rely on it for a diagram; this document
  supersedes it for the interaction flow.
- **`AuthorizationRoles.None` is numerically the *largest* value (`1024`)**, i.e. weakest — easy to misread as
  "highest tier" from the numeric value alone.
