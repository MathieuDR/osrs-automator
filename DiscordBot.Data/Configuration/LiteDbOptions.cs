namespace DiscordBot.Data.Configuration;

public class LiteDbOptions {
    public string PathPrefix { get; set; }
    public string FileSuffix { get; set; }
    public static string SectionName => "LiteDbOptions";
}
