using System;
using System.Collections.Generic;

namespace DiscordBotFanatic.Models.ResponseModels {
    public class PagedStrings : IPageableResponse {
        public string AlternatedDescription { get; set; }
        public IEnumerable<object> Pages { get; set; }
    }
}