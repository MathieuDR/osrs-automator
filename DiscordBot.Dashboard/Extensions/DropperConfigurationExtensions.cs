using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Dashboard.Models;

namespace DiscordBot.Dashboard.Extensions;

public static class DropperConfigurationExtensions {
    public static DropperGuildConfigurationModel ToRequest(this DropperGuildConfiguration record) =>
        new DropperGuildConfigurationModel {
            IsEnabled = record.IsEnabled,
            DisabledUsers = record.DisabledUsers.ToList(),
            BaseConfiguration = record.MasterConfiguration.ToRequest()
        };

    public static DropperGuildConfiguration ToRecord(this DropperGuildConfigurationModel request, DropperGuildConfiguration original) =>
        original with {
            IsEnabled = request.IsEnabled,
            DisabledUsers = request.DisabledUsers,
            MasterConfiguration = request.BaseConfiguration.ToRecord()
        };

    private static DropperConfigurationRequest ToRequest(this DropperConfiguration record) =>
        new DropperConfigurationRequest {
            AlwaysSendCollectionLogItems = record.AlwaysSendCollectionLogItems,
            BlackListedUsers = record.BlackListedUsers,
            BlackListedItems = record.BlackListedItems,
            BlackListedSources = record.BlackListedSources,
            BlackListedPlayerType = record.BlackListedPlayerType,
            WhiteListedUsers = record.WhiteListedUsers,
            WhiteListedItems = record.WhiteListedItems,
            WhiteListedSources = record.WhiteListedSources,
            WhiteListedPlayerType = record.WhiteListedPlayerType,
            IsEnabled = record.IsEnabled,
            MinimumGeValue = record.MinimumGeValue,
            MinimumHaValue = record.MinimumHaValue,
            RequireBothGeAndHaValue = record.RequireBothGeAndHaValue
        };

    private static DropperConfiguration ToRecord(this DropperConfigurationRequest request) =>
        new DropperConfiguration {
            AlwaysSendCollectionLogItems = request.AlwaysSendCollectionLogItems,
            BlackListedUsers = request.BlackListedUsers,
            BlackListedItems = request.BlackListedItems,
            BlackListedSources = request.BlackListedSources,
            BlackListedPlayerType = request.BlackListedPlayerType,
            WhiteListedUsers = request.WhiteListedUsers,
            WhiteListedItems = request.WhiteListedItems,
            WhiteListedSources = request.WhiteListedSources,
            WhiteListedPlayerType = request.WhiteListedPlayerType,
            IsEnabled = request.IsEnabled,
            MinimumGeValue = request.MinimumGeValue,
            MinimumHaValue = request.MinimumHaValue,
            RequireBothGeAndHaValue = request.RequireBothGeAndHaValue
        };
}
