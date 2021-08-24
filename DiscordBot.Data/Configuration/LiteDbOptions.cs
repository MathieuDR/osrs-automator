namespace DiscordBot.Data.Configuration {
    public class LiteDbOptions {
        public string FileName { get; set; }
        public static string SectionName => "LiteDbOptions";
    }
}
