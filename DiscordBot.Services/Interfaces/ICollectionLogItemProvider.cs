using FluentResults;

namespace DiscordBot.Services.Interfaces;

public interface ICollectionLogItemProvider {
    ValueTask<Result<IEnumerable<string>>> GetCollectionLogItemNames();
    Result ResetCache();
}
