using DiscordBot.Services.Models.MediaWikiApi;
using Refit;

namespace DiscordBot.Services.ExternalServices;

public interface IOsrsWikiApi {
    [Headers("User-Agent: DiscordBot/1.0.0 ","Accept: application/json")]
    [Get("/api.php?action=query&format=json&prop=revisions&titles={pageTitles}&rvprop=content")]
    Task<QueryResponse> GetPages(string[] pageTitles);

    [Headers("User-Agent: DiscordBot/1.0.0 ","Accept: application/json")]
    [Get("/api.php?action=query&format=json&prop=revisions&titles={pageTitle}&rvprop=content&rvslots=main")]
    Task<QueryResponse> GetPage(string pageTitle);
}
