namespace DiscordBot.Models.Configuration {
    public class BotConfiguration {
        public string Token { get; set; }

        //public string DatabaseFile { get; set; }
        public MessageConfiguration Messages { get; set; }
        public string CustomPrefix { get; set; }
    }
}