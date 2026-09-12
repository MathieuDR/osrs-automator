namespace DiscordBot.Common.Configuration;

public class MessageConfiguration {
	public List<string> WaitMessages { get; set; }
	public HostingMessages Hosting { get; set; } = new();
}
