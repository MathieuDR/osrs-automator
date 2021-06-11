using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Interfaces;

namespace DiscordBotFanatic.Services {
    public class WisOldManLogger : IWiseOldManLogger{
        private readonly ILogger<WisOldManLogger> _logger;

        public WisOldManLogger(ILogger<WisOldManLogger> logger) {
            _logger = logger;
        }
        public Task Log(LogLevel logLevel, Exception e, string message, params object[] arguments) {
            _logger.Log(logLevel, e, message, arguments);
            return Task.CompletedTask;
        }
    }
}
