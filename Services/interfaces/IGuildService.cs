using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Modules.DiscordCommandArguments;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IGuildService {
        bool HasActiveEvent(IGuild guild);
        List<GuildCustomEvent> GetActiveGuildEvents(IGuild guild);
        GuildCustomEvent InsertGuildEvent(GuildCustomEvent guildCustomEvent);
        bool EndGuildEvent(GuildCustomEvent guildCustomEvent);
        bool DoesUserHavePermission(IGuildUser user, Permissions permission);
        bool ToggleRole(IRole role, Permissions permission);
        bool AddEventCounter(IGuild guild, UserListWithImageArguments arguments);
        void RemoveCounters(GuildCustomEvent guildCustomEvent, List<GuildEventCounter> toDelete);
        Task<CreateGroupCompetitionResult> CreateGroupCompetition(string title, MetricType metric, DateTime startDate, DateTime endDate);
        Task<bool> CreateGuildCompetition(IGuildUser user, int id);
        Task<bool> DeleteGuildCompetition(int id, DateTime endDate);
    }
}