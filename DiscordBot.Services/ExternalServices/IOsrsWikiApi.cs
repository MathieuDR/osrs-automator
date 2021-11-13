using System.Threading.Tasks;
using DiscordBot.Services.Models.MediaWikiApi;
using Refit;

namespace DiscordBot.Services.ExternalServices; 

public interface IOsrsWikiApi {
    [Get("/api.php?action=query&format=json&prop=revisions&titles={pageTitles}&rvprop=content")]
    Task<QueryResponse> GetPages(string[] pageTitles);

    [Get("/api.php?action=query&format=json&prop=revisions&titles={pageTitle}&rvprop=content")]
    Task<QueryResponse> GetPage(string pageTitle);
}