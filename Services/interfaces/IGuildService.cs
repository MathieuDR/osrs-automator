using System.Collections.Generic;
using Discord;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Modules.DiscordCommandArguments;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IGuildService {
        bool HasActiveEvent(IGuild guild);
        List<GuildEvent> GetActiveGuildEvents(IGuild guild);
        GuildEvent InsertGuildEvent(GuildEvent guildEvent);
        bool EndGuildEvent(GuildEvent guildEvent);
        bool DoesUserHavePermission(IGuildUser user, Permissions permission);
        bool ToggleRole(IRole role, Permissions permission);
        bool AddEventCounter(IGuild guild, UserListWithImageArguments arguments);
        void RemoveCounters(GuildEvent guildEvent, List<GuildEventCounter> toDelete);
    }
}