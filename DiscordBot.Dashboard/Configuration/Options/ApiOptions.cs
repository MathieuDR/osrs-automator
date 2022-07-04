namespace DiscordBot.Dashboard.Configuration.Options;

public class ApiOptions {
	public int VersionMajor { get; set; }
	public int VersionMinor { get; set; }
	public string Version => $"v{VersionMajor}.{VersionMinor}";
	public string JsonRoute { get; set; }
	public string Description { get; set; }
	public string UIEndpointSuffix { get; set; }
}

public class OAuthOptions {
	public string ClientId { get; set; }
	public string ClientSecret { get; set; }
	public string RedirectUrl { get; set; }
}