namespace DiscordBot.Models;

public class PagedStrings : IPageableResponse {
    public string AlternatedDescription { get; set; }
    public IEnumerable<object> Pages { get; set; }
}
