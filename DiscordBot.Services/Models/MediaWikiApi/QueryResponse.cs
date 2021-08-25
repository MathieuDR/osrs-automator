using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DiscordBot.Services.Models.MediaWikiApi {
    public class Revision
    {
        [JsonPropertyName("contentformat")]
        public string Contentformat { get; set; }

        [JsonPropertyName("contentmodel")]
        public string Contentmodel { get; set; }

        [JsonPropertyName("*")]
        public string Content { get; set; }
    }

    public class Page
    {
        [JsonPropertyName("pageid")]
        public int Pageid { get; set; }

        [JsonPropertyName("ns")]
        public int Ns { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("revisions")]
        public List<Revision> Revisions { get; set; }
    }

    public class Query
    {
        [JsonPropertyName("pages")]
        public Dictionary<string, Page> Pages { get; set; }
    }

    public class QueryResponse
    {
        [JsonPropertyName("batchcomplete")]
        public string Batchcomplete { get; set; }

        [JsonPropertyName("query")]
        public Query Query { get; set; }
    }

        
    
}
