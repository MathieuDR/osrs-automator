# Ethics review: hosting payment tracker / "public shaming"

Reviewer: independent · 2026-09-12 · Subject: `specs/2026-09-12-hosting-payment-tracker-design.md` · advisory only; no files modified.

## 1. Is the overall idea ethically reasonable?

**Verdict: the goal is legitimate, the mechanism is aimed at the wrong people.**

You are owed money under an agreement you both made, you have carried the cost for years, and you may
withdraw the bot entirely tomorrow without notice or justification. Wanting to be paid needs no defence.

The problem is that the footer's audience and the debt's audience are different groups. Clan members running
`/kc` didn't sign the agreement, don't control the clan's money, and mostly can't make leadership do
anything. A message worded to make *them* feel bad is pressure on people who can only relay it — effective
only in proportion to how much it annoys bystanders into pestering someone else.

Two things work in the design's favour: **no individual is ever named** (it's about a server, not a person),
which keeps this far from the genuinely harmful end of shaming; and **informing the clan isn't itself
illegitimate** — they're the beneficiaries, and after years of leadership silence, "this is unpaid" is fair
disclosure. The jokes, the escalation and the guilt-framing are what turn disclosure into a campaign.

Two structural issues matter more than any single line of text:

- **Unbounded frequency.** The footer rides *every* command reply, forever. Someone using the bot 40 times a
  day sees it 40 times. Repetition, not wording, converts "notice" into "harassment-feeling". Dedupe it:
  once per channel per few hours, or once per user per day.
- **No terminal state.** Tiers climb 0 → 7 → 30 → 90 and then just continue, indefinitely, while you keep
  paying. A grievance broadcast with no off-ramp guarantees the bleed continues while relations degrade.

## 2. Footer texts and tiers (§3.6)

Nothing is abusive or likely to cause real harm — no individual targeted, no slurs, no threats. The problems
are misdirection and one bit of coyness.

**Keep:** all three `Overdue[0]` texts (factual, mild, self-deprecating — the right register for the whole
feature); `🪦 …running on fumes and spite` and `⏳ …a charity case` (self-directed, no target); and above all
`🚨 Month-plus overdue. Leadership, this footer is for you.` — **the best line in the set**, because it names
the responsible party and tells everyone else they aren't the target. Make it the template for every tier.

**Soften:**

- `😬 Every command you run costs someone else money.` — tells the individual user they're personally running
  up your bill; aimed exactly at the person who can't fix it, and the line most likely to make someone just
  stop using the bot. → `😬 {days} days unpaid. This is a leadership thing, not a you thing — but now you know.`
- `🪙 A few coins from each of you would fix this.` — converts a leadership debt into a perpetual crowdfund
  pitch on every reply. Ask once in a channel if you want that. → `🪙 {days} days overdue. Hosting is €X/month,
  split 50/50. That's the whole story.`
- `🚨 {days} DAYS OVERDUE … kept alive out of pity. Pay the man.` — drop "pity" (members aren't pitying
  anyone). → `🚨 {days} days overdue. Still running, still unpaid. Leadership knows.`

**Cut:**

- `☠️ {server} is officially freeloading. Somebody screenshot this for the leaders.` — "freeloading" is
  name-calling applied to everyone who reads it, including people who'd have paid if asked; and "screenshot
  this for the leaders" explicitly conscripts bystanders against their own leadership. → `☠️ {days} days
  unpaid. Hosting for {server} ends {date} unless this is settled.`
- `☠️ Quarter of a year unpaid. Commands may start "failing". Coincidence.` — the scare quotes and wink are
  the one genuinely dishonest note in the spec. A coy hint is worse than either silence or a real warning.
  → `☠️ {days} days unpaid. From {date} the bot starts refusing commands here. Not a bug.`
- `NeverPaid`, even as a commented example — see below.

**A design bug with ethical weight:** this is a multi-guild bot (`DiscordSocketClient.Guilds`, per-guild DBs,
autocomplete over every guild). Other servers that added the bot never agreed to pay anything. `NeverPaid` is
one global config string away from shaming them for a bill that doesn't exist. Default-empty mitigates but
doesn't fix it — make shaming **opt-in per guild** (`ShamingEnabled` on `GuildHostingState`, default `false`,
set by hand for the one clan).

**Also:** §3.4 puts the footer on the dispatcher's *failure* embed, so a user hitting a real bug gets an error
plus a guilt trip. Exclude failure embeds.

## 3. Degraded mode (§3.7)

**Verdict: the principle is defensible; this shape is the worst available implementation. Don't ship as specified.**

You can switch the bot off completely, so switching it partly off is a lesser included right. Degrading a
gift to signal the gift is ending isn't dishonest, provided it's announced and honest about the cause.

The shape is the problem. **Random 20% is the most user-hostile way to do this:** it's indistinguishable from
a flaky bot, so you'll get bug reports and have to either lie or explain to the wrong person; it taxes users
with retries ("Please retry (or pay)"); it's unpredictable, so it reads as caprice rather than consequence,
and consequences motivate while caprice just annoys; and it punishes usage, so the heaviest users — usually
the most invested members, often your allies — absorb the most of it.

90 days isn't the issue; after years of non-payment that's generous. The issue is that *deterministic*
degradation beats it at everything: disable the non-essential command set and say so; or restrict to a window
("unpaid hosting: bot answers 18:00–22:00"); or simplest and best, stop at the banner and let the bot leave on
the announced date. All are honest, predictable, equally motivating, and can't be mistaken for a bug.

If you ship it anyway, minimum conditions: (a) a dated plain-language warning at least 30 days before it
starts, naming the exact date; (b) **ephemeral** failures — answer open question 5 "ephemeral", since public
failures add embarrassment and channel noise but zero extra pressure on leadership; (c) a hard end date where
the bot leaves rather than degrading forever; (d) drop "retry" — if you've decided not to run it, don't make
them ask twice.

## 4. Discord ToS / Developer Policy

**Verdict: low practical risk; one message set should change regardless. Confidence: moderate on the
reasoning, low on predicting enforcement. I have not consulted the current policy text and am not quoting it.**

Broadly, Discord's developer terms require you not to use the API to harass users and not to mislead people
about what your application does. Against that:

- **The footer.** Server-level commentary about a bill, no individual targeted. Enforcement realistically
  follows user reports, and nobody reports a snarky hosting footer. Low risk.
- **Degraded mode, honesty.** `⚠️ Command failed: insufficient hosting funds` is **not deceptive**; it did fail, and that is literally why.
- **But two of the five defaults are deceptive**, and this is the clear finding in this section: `🔌 The
  server this bot lives on politely declined to run that` and `🪫 Out of budget for this command. Retry; free
  tier is flaky` both invent a *technical* cause that isn't happening. There's no free tier, nothing is
  flaky, the host declined nothing. That's a false statement of fact about your own software, told to people
  with no way to check. Cut both. `🎲 Rolled a 1 on the hosting dice` is fine — it's random and says so.
  Rule: every degraded message names the real cause.
- Minor: a footer on every reply could in principle be called spammy. Remote; deduplication removes it.

## 5. Alternatives, ranked

Name the goals plainly first. **Getting paid** is, after years and "two or three times after a lot of
nagging", unlikely — nothing here changes that probability. **Stopping the bleed** is solved instantly by
turning the bot off. The shaming design serves a third, unstated goal: expressing entirely justified
irritation. That's a real thing to want, and one honest message is cheaper than a permanent apparatus.

1. **Sunset with a date.** One post in the clan channel plus a leadership ping: "hosted since {year}, the
   agreed split hasn't been paid since {date}, it shuts down {date+60d} unless it is" — then a plain dated
   countdown in the footer. Ends the bleed for certain, creates the only incentive that ever works (a real
   deadline with a real loss), zero collateral, final either way. **Do this one.**
2. **Hand over the hosting.** You already have Docker and a flake in-repo. Offer the image and compose file,
   or point the bot at a host they pay directly. Costs you nothing further, keeps the bot alive for members
   who like it, moves the bill to the people who owe it. Pairs perfectly with #1 as the "or else, here" branch.
3. **One post in the leadership channel, then stop.** Cheap, correctly targeted, no collateral. Weak alone
   given the history, but it's the right first step of #1.
4. **Plain non-jokey status line** (due date + shutdown date, no escalation, deduped). Good as the *mechanism*
   for #1's countdown; useless standalone — a permanent unchanging notice becomes wallpaper within a week.
5. **Absorb it / downsize the host.** If this is €3/month and you still like these people, the feature costs
   more in build time and goodwill than the money. Worth pricing before building.
6. **The proposed design.** Last. Maximises collateral, has no end state, keeps you paying throughout, and is
   the option most likely to end with leadership defensive rather than paying. Strictly dominated by #1 at
   both stated goals.

## 6. Final recommendation

**Ship most of it, with changes — but not the shaming ramp or degraded mode as designed.** Sections 3.1–3.5
(data model, service, owner-only `/hosting` command, twice-per-term reminder job) are plain bookkeeping with
no ethical content: build them as specified, they're useful whatever you decide about the clan. For the
guild-facing part, replace the escalating ladder and the 20% random-failure mode with a single honest
countdown — pick a shutdown date, announce it once in the clan and once to leadership, run a plain dated
footer until then (deduplicated, opt-in per guild), then actually leave. That gets you everything the shaming
design reached for (the clan knows, leadership can't claim they weren't told, the pressure is real because
the deadline is real) while ending the bleed on a date you control instead of funding an indefinite grudge.
The strongest argument against the current design isn't that it's cruel; it's that it keeps you paying for a
bot whose main new feature is telling people you're unhappy about paying for it.
