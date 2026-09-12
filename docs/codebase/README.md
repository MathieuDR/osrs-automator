# Codebase documentation (for AI agents and humans)

Generated 2026-09-12 against `master` @ `4edacce`. These documents describe the code **as it is**, with
file/class references, so an agent can make changes without re-reading the whole solution. When code in a
referenced area changes, update the matching document.

| # | Document | Read this when you need to… |
|---|----------|------------------------------|
| 01 | [Host, startup & configuration](01-host-startup-and-config.md) | understand `Program.cs`/`Bot.cs`, DI wiring and lifetimes, `appsettings` sections (incl. `Bot:TeamConfiguration` owner guild/user), Discord client setup, Docker, how to build/test. |
| 02 | [Command system](02-command-system.md) | add or change a slash command. Explains the legacy `Interactive` vs current `Interactive2` (Definition / Request / Handler + MediatR) systems, how commands get registered to Discord (`/commands`), authorization (`CommandAuthorizationService`, owner bypass), reply builders, and a copy-paste recipe. |
| 03 | [Data layer](03-data-layer.md) | touch persistence. LiteDB file layout (one `.db` per guild + `common`), `LiteDbManager`, repository/factory/strategy stack, all models, migrations, recipe for a new collection. |
| 04 | [Services & jobs](04-services-and-jobs.md) | change business logic or scheduled behaviour. Service inventory, `IDiscordService` API (how to post into a channel), Quartz job hierarchy and per-guild job configuration, domain features. |
| 05 | [Dashboard, WOM connector & misc](05-dashboard-wom-connector-and-misc.md) | know what the peripheral projects are (Blazor dashboard, Wise Old Man API client, `WOMReader`, dead placeholders). |
| 06 | [Memory investigation](06-memory-leak-investigation.md) | understand the memory footprint. Measured LiteDB behaviour, ranked causes, remediation options. Input to the LiteDB lifecycle spec. |

## Specs derived from these docs

- `docs/superpowers/specs/2026-09-12-litedb-lifecycle-design.md` — close guild databases when idle (memory footprint).
- `docs/superpowers/specs/2026-09-12-hosting-payment-tracker-design.md` — owner-only hosting payment tracker with reminder and footer.

## Quick facts that trip people up

- The database is **LiteDB 5.0.16**, not SQLite. Direct connection mode, one file per guild, exclusive file lock while open. `LiteDbManager` uses reference-counted leases; databases are closed when no longer in use (default) or held open indefinitely (legacy option).
- The bot process has **no `IHost`**: `Program.cs` builds a `ServiceProvider` and parks on `Task.Delay(-1)`. Nothing is disposed on shutdown.
- There is **no persisted list of guilds**; "registered guilds" = whatever `DiscordSocketClient.Guilds` reports.
- `Bot:TeamConfiguration` (`GuildId`, `OwnerId`) is the only "owner" concept. `OwnerId` bypasses all authorization; roles `BotOwner`/`BotAdmin`/`BotModerator` are effectively owner-only.
- New Interactive2 commands are discovered by reflection but are **not pushed to Discord** until someone runs `/commands` and registers them (globally or per guild).
- Two command systems coexist; write new code only in `DiscordBot/Commands/Interactive2/`.
