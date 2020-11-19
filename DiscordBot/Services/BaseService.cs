using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
    public abstract class BaseService {
        public ILogService Logger { get; }

        public BaseService(ILogService logger) {
            Logger = logger;
        }
    }
}