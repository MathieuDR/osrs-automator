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
				"💸 {server} has not paid its hosting bill. Due {date}. The bot runs on goodwill now.",
				"🧾 Hosting invoice for {server}: unpaid since {date}.",
				"💸 Day {days} of {server} not paying for hosting. Just saying."
			}
		},
		new HostingTier {
			MinDays = 7,
			Texts = new List<string> {
				"😬 {days} days without paying for hosting. Every command you run costs someone else money.",
				"📉 {server} is {days} days late on hosting. The bot noticed. So did everyone else.",
				"🪙 {days} days overdue. A few coins from each of you would fix this."
			}
		},
		new HostingTier {
			MinDays = 30,
			Texts = new List<string> {
				"🚨 {days} DAYS OVERDUE. This bot is being kept alive out of pity. Pay the man.",
				"⏳ {days} days. At this point the bot is a charity case.",
				"🚨 Month-plus overdue. Leadership, this footer is for you."
			}
		},
		new HostingTier {
			MinDays = 90,
			Texts = new List<string> {
				"☠️ {days} days. {server} is officially freeloading. Somebody screenshot this for the leaders.",
				"🪦 {days} days overdue. The bot is running on fumes and spite.",
				"☠️ Quarter of a year unpaid. Commands may start \"failing\". Coincidence."
			}
		}
	};

	public static readonly IReadOnlyList<string> Degraded = new List<string> {
		"💸 Not running that one: {server} has not paid for hosting in {days} days. Nothing is broken. Retry, or pay.",
		"🛑 Command withheld: this server's hosting bill is {days} days overdue. Retry, or nudge whoever holds the clan coffers.",
		"💤 The bot works when the hosting gets paid. {days} days overdue. Retry in a moment.",
		"🧾 Unpaid hosting ({days} days) means about 1 in 5 commands takes a nap. This was one of them. Retry.",
		"🎲 Rolled the unpaid-hosting dice and lost. Not a bug, just {days} days of unpaid hosting. Retry."
	};
}
