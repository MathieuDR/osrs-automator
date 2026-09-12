# Runbook — OSRS Discord Automator

Operational notes for the person hosting the bot. Server-side config lives in the `nix-dock` repo (`services/osrs-automator/`); the bot runs as the podman container `osrs-automator` (systemd unit `podman-osrs-automator.service`, `Restart=always`).

## 1. Where things are

| what | where |
|---|---|
| image | `ghcr.io/mathieudr/osrs-automator:latest` (+ `:sha-<short git sha>` per release) |
| databases (LiteDB, one file per guild + common) | server `/srv/osrs-automator/data/db_<guildId>_prod.db`, `db_common_prod.db` |
| base config | nix-dock `services/osrs-automator/appsettings.json` → `/etc/osrs-automator/appsettings.json` → `/app/appsettings.json` |
| secrets/override (token, WOM key, `TeamConfiguration`, `FileSuffix=prod`) | age secret `appsettings.override.json` → `/app/appsettings.production.json` (wins over the base file) |
| logs | `podman logs osrs-automator`; JSON file log inside the container at `/app/logs/osrs_bot.log` |
| backups | restic (b2) of the data dir + base appsettings; manual tarballs in `/srv/osrs-automator/backups/` and locally `~/osrs-automator-backups/` |
| update / rollback scripts on the server | `update-osrs-automator`, `rollback-osrs-automator <digest>`; log in `/var/log/container-updates/osrs-automator.log` |

Guilds as of 2026-09-12: `403539795944538122` = owner/manager server; `784510502474219530` = the clan (the only database with real data); `430053964927729684` and `806544893487874078` are 8 KB stubs.

## 2. Release a new version

```bash
# in osrs-automator, on a clean master
podman login ghcr.io -u MathieuDR      # once, classic PAT with write:packages
just release                            # build + push :latest and :sha-<short>

# in nix-dock, only if appsettings.json changed
just rebuild                            # ships /etc/osrs-automator/appsettings.json

# on the server
ssh root@$SERVER_IP
systemctl stop podman-osrs-automator.service
tar czf /srv/osrs-automator/backups/data-$(date +%Y%m%d-%H%M).tgz -C /srv/osrs-automator data
systemctl start podman-osrs-automator.service   # optional; update-… restarts anyway
update-osrs-automator                   # pulls :latest, restarts, logs the rollback command
podman logs --tail 50 osrs-automator
```

Rollback: `rollback-osrs-automator <old digest>` (the digest is printed by the update script and stored in its log). Config-only rollback for the database lifecycle change: set `LiteDbOptions:CloseWhenUnused` to `false` in appsettings, `just rebuild`, restart.

Stopping the container is optional for a consistent backup since the lease lifecycle change: databases are closed whenever no command or job is using them. Stop anyway if you want certainty.

## 3. Health checks

- `systemctl is-active podman-osrs-automator.service`
- `podman stats --no-stream osrs-automator` — memory. Baseline before the lease change: ~225 MB. Expect it to plateau lower and stay flat.
- Every 30 min the bot logs `Memory report: workingSet=… gcHeap=… handles=… openDatabases=…`. `openDatabases` should sit at 0–2 between interactions. If it tracks the guild count again, something holds repositories open.
- Slash commands registered: the bot re-syncs known commands at startup (`Updating command … with hash …` log lines).

## 4. Hosting payment tracker

All commands are owner-only (`Bot:TeamConfiguration:OwnerId`) and only work inside the owner/manager server (`Bot:TeamConfiguration:GuildId`). `server` options offer autocomplete over the guilds the bot is in; pick by name.

### First-time setup (once per deploy of a new bot install)
1. In the manager server run `/commands`, select `hosting`, register it to the manager server only (not global).
2. `/hosting remind channel:#<channel>` — where the owner reminders land.

### Record a payment
`/hosting paid server:<clan> date:YYYY-MM-DD months:12 note:"who / how much"`
- `date` defaults to today; `months` defaults to 12 (1–60).
- Recording a payment moves the due date and re-arms both owner reminders.
- Example for the current situation (clan last paid in August 2025): `/hosting paid server:<clan> date:2025-08-DD months:12`. Due date becomes August 2026, so the clan is already overdue: the footer appears immediately (tier 7 or 30 depending on the day), the first 09:00 job run sends you the single "due" reminder, and degraded mode starts once 90 days past due (roughly November 2026) unless a payment is recorded.

### See the state
- `/hosting status` — every guild: last paid, due, days left/overdue, footer on/off, most overdue first.
- `/hosting status server:<clan>` — details and last 10 payments.

### Turn the shaming off / on for one server
`/hosting footer server:<clan> enabled:false` — disables both the footer and degraded mode for that server. `enabled:true` to restore. Payments keep being tracked either way.

### What the clan sees
- Overdue: the last line of every command reply reads `Requested by <user> · <text>`; on paginated replies the text is the last line of the embed body. Tier by days overdue: 0, 7, 30, 90. Random text per tier from `Bot:Messages:Hosting:Overdue`.
- ≥ 90 days overdue: each slash command has a `FailureChance` (default 0.2) chance to be withheld with a text from `Bot:Messages:Hosting:Degraded:Texts` that names unpaid hosting as the cause. Never for buttons, autocomplete, DMs, the owner, the manager server, or `/hosting` itself.
- Paid up or in the last month: nothing.

### Owner reminders
Daily 09:00 Europe/Berlin. Exactly two per due date: once when 30 days remain (or the first run after that if the bot was down), once on/after the due date. Nothing after that until a new payment is recorded. Sent as an @mention in the reminder channel.

### Change the texts
Edit `Bot:Messages:Hosting` in nix-dock `services/osrs-automator/appsettings.json`, `just rebuild`, restart the container. Placeholders: `{server}` (guild name), `{days}`, `{date}` (due date), `{paidDate}`. Code defaults apply when a list is missing or empty. Gotcha: .NET merges JSON arrays by index across config files, so an override file with fewer tiers keeps the base file's extra tiers — override the whole list or keep tier positions aligned. `Degraded:Enabled=false` turns degraded mode off everywhere.

## 5. Troubleshooting

| symptom | check |
|---|---|
| `/hosting` missing in Discord | not registered yet: `/commands` → `hosting` → manager server. Restarts only re-sync already-registered commands. |
| `/hosting` says "Only available in the bot owner's server" | `TeamConfiguration:GuildId` in the override does not match the server you're in |
| footer never shows | guild not overdue, footer disabled for that guild, or no payment recorded (no payment = silent by default) |
| command "failed" with a hosting text | intended: degraded mode; users retry |
| `Could not read hosting settings` warning in logs | common DB unreadable; footer falls back to silent, commands keep working |
| memory creeping again | check `openDatabases` in the memory report; as a stopgap set `CloseWhenUnused=false` is NOT the fix (that is the old behaviour) — look for a repository not disposed |
| database file locked for backup | a command/job is running; retry, or stop the container first |
