using System;

namespace WiseOldManConnector.Models.API.Responses {
    internal class SearchResponse : BaseResponse {
        // TODO ADD JSONPROPERTTIES!
            public int Id { get; set; }
            public string Username { get; set; }
            public string Type { get; set; }
            public DateTime? LastImportedAt { get; set; }
            public DateTime RegisteredAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
    }
}