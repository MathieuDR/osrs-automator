using System.Collections.Generic;

namespace DiscordBot.Models.ResponseModels {
    public class PagedStrings : IPageableResponse {
        public string AlternatedDescription { get; set; }
        public IEnumerable<object> Pages { get; set; }
    }
}