using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Dashboard.Models;

namespace DiscordBot.Dashboard.Extensions;

public static class DropperConfigurationExtensions {
    public static DropperGuildConfigurationModel ToRequest(this DropperGuildConfiguration record) {
        return new DropperGuildConfigurationModel() {
            IsEnabled = record.IsEnabled,
            DisabledUsers = record.DisabledUsers.ToList()
        };
    }

    public static DropperGuildConfiguration ToRecord(this DropperGuildConfigurationModel request, DropperGuildConfiguration original) {
        return original with {
            IsEnabled = request.IsEnabled,
            DisabledUsers = request.DisabledUsers
        };
    }
}
