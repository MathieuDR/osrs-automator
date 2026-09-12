# Hosting payment tracker — manual verification checklist

Manual checks for the hosting payment tracker
(`docs/superpowers/specs/2026-09-12-hosting-payment-tracker-design.md`), to run once against a real bot
process with a dev/test guild, after `dotnet test` has already passed. These exercise the parts that only
show up against the live Discord gateway: command registration, the reply-pipeline footer hook, the
degraded-mode random draw, and the scheduled reminder job.

## 0. Register the command

1. Deploy/run the bot with the branch's build.
2. As the configured `Bot:TeamConfiguration:OwnerId`, run `/commands` in the owner guild
   (`Bot:TeamConfiguration:GuildId`).
3. Pick `hosting` from the command select menu, then choose the owner guild from the guild select menu
   (not "Register globally" — `/hosting` is owner-guild-only per §3.3 of the spec).

**Expected:** `/hosting paid`, `/hosting status`, `/hosting footer`, `/hosting remind` are now available as
slash commands in the owner guild. Running `/hosting` in any other guild the bot is in still 404s/doesn't
autocomplete, since it was never registered there.

## 1. Footer appears once a server is overdue

1. In the owner guild, run `/hosting paid server:<a dev/test clan server> date:<13 months ago, yyyy-mm-dd>
   months:12`. This backdates the due date to one month in the past (overdue).
2. In that dev/test clan server (not the owner guild), run `ping2 normal` (a plain text-only reply).
3. In the same server, run `count ranking` (or any other embed-based command).

**Expected:**
- `ping2 normal`'s reply ends with a `-#` subtext line matching one of the `Bot:Messages:Hosting:Overdue`
  tier texts for ~30 days overdue (the `MinDays: 0` tier applies to any `DaysOverdue >= 0`).
- `count ranking`'s embed footer reads `Requested by <you> · <same overdue text>`.
- Both texts include the substituted `{server}`/`{days}`/`{date}` values, not literal placeholders.

## 2. Degraded mode fires roughly 1 in 5 times once ~90 days overdue

1. In the owner guild, run `/hosting paid server:<the same test server> date:<4 months (~120 days) ago>
   months:1`. This makes the server ~90 days overdue against the default `Degraded.MinDaysOverdue = 90`.
2. In that test server, run any non-`hosting` slash command (e.g. `ping2 normal`) roughly 15-20 times in a
   row, as a user who is **not** the bot owner.

**Expected:** roughly 1 in 5 of the runs (not every run, not a fixed count) return a public (non-ephemeral)
failure embed with one of the `Bot:Messages:Hosting:Degraded:Texts` messages instead of the command's
normal output — the message must read as "hosting is unpaid, nothing is broken, retry", never as a
technical error. The failure embed itself still carries the overdue footer (from check 1, since the
degraded reply goes through the normal `CreateEmbedBuilder`/`RespondAsync` pipeline). The remaining ~4 in 5
runs behave completely normally.

## 3. Footer toggle silences everything for that server

1. In the owner guild, run `/hosting footer server:<the same test server> enabled:false`.
2. In that test server, run `ping2 normal` and `count ranking` several times, again as a non-owner user.

**Expected:** no footer text on any reply (plain replies have no trailing `-#` line, embeds have only
"Requested by …" with no `· …` suffix), and degraded mode never fires regardless of how overdue the server
is — the per-guild footer toggle is also the degraded-mode kill switch (`HostingService.ShouldDegrade`
returns `false` whenever `FooterEnabled == false`). Re-enable with
`/hosting footer server:<test server> enabled:true` afterwards if continuing to check 4/5.

## 4. Reminder job sends once, then nothing on a second run

1. In the owner guild, run `/hosting paid server:<a test server> date:<11 months ago> months:12` — this
   puts the server ~1 month from due (`Upcoming` window) or just past due (`Due` window) depending on the
   exact date picked; pick a date so `DaysOverdue` lands at or after `-30` (e.g. exactly 11 months ago with
   a 12-month term gives `DaysOverdue` close to `-30`).
2. Confirm (or set) the reminder channel: either it was auto-set to the channel `/hosting paid` was run in
   the first time ever, or run `/hosting remind channel:<a channel in the owner guild>` explicitly.
3. Temporarily reschedule `HostingReminderJob` to fire immediately (e.g. via `/job queue`-style ad-hoc
   trigger if wired up for this job, or restart the process with the cron trigger's start time shifted
   momentarily, or simply wait for the next 09:00 `Europe/Berlin` run) — whichever is easiest given the
   deployment.
4. After the job fires once, trigger it again a second time (same day) without recording any new payment.

**Expected:**
- First run: a message tagging `<@OwnerId>` arrives in the reminder channel, with an embed titled "Hosting
  payments" and one field for the test server (`due <date> (N days left | N days overdue)`), colored orange
  if only "upcoming" reminders are in the batch or red if any guild is `Due`.
- Second run (same day, no new payment): **no** message is sent for that guild — `UpcomingReminderSentForDueOn`/
  `DueReminderSentForDueOn` was marked after the first send, so `HostingReminderJob.Decide` returns `None`
  for it until a new payment produces a new due date.

## 5. Watch the logs

Throughout checks 1-4 (and generally, once this branch is deployed), tail `logs/osrs_bot.log` and confirm:
- A `Memory report` line appears roughly every 30 minutes (`MemoryReportJob`, unrelated to hosting but
  useful confirmation the Quartz scheduler and logging pipeline are both alive).
- A `degraded mode` line (`InteractiveCommandHandlerService`: `"[{ctx}] degraded mode: refusing command"`,
  Information level) appears for every degraded-mode refusal from check 2, with the guild and command name
  in the log context — use this to confirm the ~1-in-5 rate matches what was actually observed in Discord,
  and that no degraded refusal happened outside a non-owner, non-`hosting`, `SocketSlashCommand` context.
