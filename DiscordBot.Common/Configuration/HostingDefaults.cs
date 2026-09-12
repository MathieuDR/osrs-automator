namespace DiscordBot.Common.Configuration;

/// <summary>
/// Fallback wording used when <see cref="HostingMessages.Overdue"/> or
/// <see cref="DegradedMode.Texts"/> is null or empty (see spec §3.6/§3.7). <see cref="HostingMessages.NeverPaid"/>
/// has no default - null/empty means "stay silent" by design.
/// </summary>
public static class HostingDefaults {
	public static readonly IReadOnlyList<HostingTier> Overdue = new List<HostingTier> {
		new HostingTier {
			MinDays = 0,
			Texts = new List<string> {
				"💸 {server} hasn't settled the hosting bill. Due {date}. The bot runs on goodwill for now.",
				"🧾 Invoice's out for {server}: unpaid since {date}.",
				"💸 Day {days} of {server} sitting on the tab. Just noting it.",
				"📬 Friendly reminder: the server costs money, and {server} hasn't sent any.",
				"🗺️ Hosting invoice for {server} sitting unopened, like a clue scroll nobody's finishing.",
				"💰 Still waiting on {server} to buy the bot a bond. No bond, no upkeep."
			}
		},
		new HostingTier {
			MinDays = 7,
			Texts = new List<string> {
				"😬 {days} days unpaid. Every command you run is quietly funded by someone else's card.",
				"📉 {server} is {days} days behind on the tab. The bot noticed. So did everyone else.",
				"🪙 {days} days overdue. A few coins from each of you would clear the invoice right up.",
				"📆 {days} days late on the rent. Not urgent yet. Getting there.",
				"🐍 {days} days unpaid. Zulrah's rotations are more predictable than this server settling up.",
				"🧟 {days} days behind on the tab. The bot's just afk-woodcutting through it at this point."
			}
		},
		new HostingTier {
			MinDays = 30,
			Texts = new List<string> {
				"🚨 {days} days overdue. This bot is being kept alive out of pure pity. Pay the man.",
				"⏳ {days} days unpaid. At this point the upkeep is basically a charity case.",
				"🚨 Month-plus overdue on the invoice. Leadership, this footer is for you.",
				"💢 {days} days unpaid. The bill's old enough to have opinions.",
				"💀 {days} days unpaid. The bot's on its way to Death's office, and Death's fee is steeper than hosting.",
				"🏹 {days} days overdue. Hits harder than a Zulrah mage phase with prayer down."
			}
		},
		new HostingTier {
			MinDays = 90,
			Texts = new List<string> {
				"☠️ {days} days unpaid. {server} is officially freeloading. Somebody screenshot this for the leaders.",
				"🪦 {days} days overdue. The bot is running on fumes and spite.",
				"☠️ A quarter of a year unpaid. Commands may start \"failing\". Coincidence.",
				"🐍 {days} days overdue. Keeping the lights on has become somebody's personal charity project.",
				"🏴 {days} days unpaid. Full wilderness rules now — everything's on the line and nobody's got protect item on.",
				"🎣 {days} days overdue. Waiting on payment here feels rarer than a barrel drop."
			}
		}
	};

	public static readonly IReadOnlyList<string> Degraded = new List<string> {
		"💸 Not running that one: {server} hasn't paid for hosting in {days} days. Nothing is broken. Retry, or pay.",
		"🛑 Command withheld: the tab's been open for {days} days. Retry, or nudge whoever holds the coffers.",
		"💤 The bot works once the bill's paid. {days} days overdue. Retry in a moment.",
		"🧾 Unpaid hosting ({days} days) means about 1 in 5 commands takes a nap. This was one of them. Retry.",
		"🎲 Rolled the unpaid-invoice dice and lost. Not a bug, just {days} days overdue. Retry.",
		"⚙️ Skipped that one on purpose. {days} days of unpaid upkeep will do that. Retry.",
		"🥱 The bot's tired of working for free. {days} days overdue. Retry, and maybe settle up in the meantime.",
		"⚰️ Command's off to Death's office. {days} days unpaid buys a gravestone instead of a response. Retry.",
		"🐌 That one didn't make it through. {days} days unpaid hosting moves slower than a giant snail. Retry."
	};
}
