using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
    public abstract class BaseService {
        public BaseService(ILogService logger) {
            Logger = logger;
        }

        public ILogService Logger { get; }
    }
}