using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

public abstract class BaseService {
    protected readonly ILogger Logger;

    public BaseService(ILogger logger) {
        Logger = logger;
    }
}
