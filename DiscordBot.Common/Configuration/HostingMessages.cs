namespace DiscordBot.Common.Configuration;

public class HostingMessages {
	// The .NET configuration binder appends to a pre-populated list rather than replacing it, so
	// these stay nullable and the defaults live in HostingDefaults, applied at read time when the
	// bound list is null or empty.
	public List<HostingTier>? Overdue { get; set; }
	public List<string>? NeverPaid { get; set; }
	public DegradedMode Degraded { get; set; } = new();
}

public class HostingTier {
	public int MinDays { get; set; }
	public List<string> Texts { get; set; } = new();
}

public class DegradedMode {
	public bool Enabled { get; set; } = true;
	public int MinDaysOverdue { get; set; } = 90;
	public double FailureChance { get; set; } = 0.2;
	public List<string>? Texts { get; set; }
}
