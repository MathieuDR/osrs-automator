using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class Delta {
        public int Start { get; set; }
        public int End { get; set; }

        [JsonProperty("Delta")]
        public int Gained { get; set; }
    }
}