using System;

namespace DiscordBot.Models.Enums {
    [Flags]
    public enum BotPermissions {
        None = 0,
        EventManager,
        CompetitionManager
    }
}