# Hosting shame visibility — manual checklist

After deploying, run `/commands` in the owner guild if `footers`/`templates` don't show up
yet (new subcommands need re-registration, same as the original hosting feature).

1. Pick a clan guild that's overdue (or seed one via `/hosting paid` with an old date +
   short term).
2. Run a few unrelated commands in that guild as different users to generate footer
   activity.
3. `/hosting footers <that server>` — confirm it lists the commands run, newest first,
   paginated once you pass 10 entries.
4. `/hosting templates` (no server) — confirm every configured Never-paid/Overdue-tier/
   Degraded text shows up with a non-zero count for whichever ones you just triggered.
5. `/hosting templates <that server>` — confirm the front page states the correct current
   tier for that guild.
6. If that guild is old enough to be in degraded-mode range (90+ days overdue by default),
   run enough commands to trigger a few degraded refusals, then `/hosting status <that
   server>` — confirm the "Degraded mode: X denied / Y allowed (Z%)" line appears and the
   percentage is sane.
7. Confirm `/hosting status <a guild that's never been overdue>` does NOT show the
   degraded-mode line (count should be 0/0, so it should be hidden).
