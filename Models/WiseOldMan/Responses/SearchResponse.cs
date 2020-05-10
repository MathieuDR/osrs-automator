using System;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses {
    public class SearchResponse : BaseResponse {
            public int Id { get; set; }
            public string Username { get; set; }
            public string Type { get; set; }
            public DateTime? LastImportedAt { get; set; }
            public DateTime RegisteredAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
    }
}