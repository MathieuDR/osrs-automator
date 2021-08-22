using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services {
    public abstract class BaseService {
        private readonly ILogger _logger;

        public BaseService(ILogger logger) {
            _logger = logger;
        }
    }
}