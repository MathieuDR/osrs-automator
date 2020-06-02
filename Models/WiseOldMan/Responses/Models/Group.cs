using System;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class Group {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("clanChat")]
        public string ClanChat { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}