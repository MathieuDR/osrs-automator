using System;
using System.Collections.Generic;

namespace DiscordBot.Models.ResponseModels {
    public interface IPageableResponse {
        public string AlternatedDescription { get; set; }
        public IEnumerable<Object> Pages { get; set; }
    }
}