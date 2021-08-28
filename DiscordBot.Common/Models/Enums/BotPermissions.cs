using System;

namespace DiscordBot.Common.Models.Enums {
    [Flags]
    public enum BotPermissions {
        None = 1,
        EventManager = 2,
        CompetitionManager = 4
    }
}
