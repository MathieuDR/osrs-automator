using System.Threading.Tasks;
using Discord;

namespace DiscordBotFanatic.Services.interfaces {
    public interface ILogService {
        Task LogDebug(LogMessage message);
        Task LogDiscordClient(LogMessage message);
        Task LogCommand(LogMessage message);
    }
}