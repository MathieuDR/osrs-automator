namespace DiscordBot.Services.Helpers;

// .NET 7 predates TimeProvider, so a minimal seam is rolled here so tests can pin "now".
public interface IClock {
	DateTime UtcNow { get; }
}

public sealed class SystemClock : IClock {
	public DateTime UtcNow => DateTime.UtcNow;
}
