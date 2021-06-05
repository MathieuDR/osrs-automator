using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using DiscordBotFanatic.Models;

namespace DiscordBotFanatic.Services.interfaces {
    public interface ICounterService {
        int TotalCount(IGuildUser user);
        int Count(IGuildUser user, IGuildUser requester, int additive, string reason);
        UserCountInfo GetCountInfo(IGuildUser user);
        List<UserCountInfo> TopCounts(SocketGuild contextGuild, int quantity);
    }
}
