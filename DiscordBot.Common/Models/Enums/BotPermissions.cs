using System;

namespace DiscordBot.Common.Models.Enums {
    [Flags]
    public enum BotPermissions {
        None = 0,
        EventManager,
        CompetitionManager
    }
}