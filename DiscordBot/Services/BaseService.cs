using DiscordBot.Services.interfaces;

namespace DiscordBot.Services {
    public abstract class BaseService {
        public BaseService(ILogService logger) {
            Logger = logger;
        }

        public ILogService Logger { get; }
    }
}