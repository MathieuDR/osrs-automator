using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Services.ExternalServices;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Models.MediaWikiApi;
using DiscordBot.Services.Parsers;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services; 

internal class CollectionLogItemProvider : BaseService, ICollectionLogItemProvider {
    private const string PageTitle = "Collection log";
    private readonly IOsrsWikiApi _wikiApi;

    private IEnumerable<string> _collectionLogItems;

    public CollectionLogItemProvider(ILogger<CollectionLogItemProvider> logger, IOsrsWikiApi wikiApi) : base(logger) {
        _wikiApi = wikiApi;
    }

    public async ValueTask<Result<IEnumerable<string>>> GetCollectionLogItemNames() {
        if (_collectionLogItems is null) {
            var wikiResult = await GetItemsFromApi();
            if (wikiResult.IsFailed) {
                return wikiResult;
            }

            _collectionLogItems = wikiResult.Value;
        }

        return Result.Ok(_collectionLogItems);
    }

    public Result ResetCache() {
        _collectionLogItems = null;
        return Result.Ok();
    }

    private async Task<Result<IEnumerable<string>>> GetItemsFromApi() {
        var queryResult = await _wikiApi.GetPage(PageTitle);

        var page = queryResult.Query.Pages.FirstOrDefault();
        if (page.Equals(default(KeyValuePair<string, Page>))) {
            return Result.Fail("No pages in api request");
        }

        var revision = page.Value.Revisions.FirstOrDefault();

        if (revision is null) {
            return Result.Fail("No revision in page in api request");
        }

        return MediaWikiContentToItemsParser.GetItems(revision.Content);
    }
}