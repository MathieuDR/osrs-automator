using System;

namespace DiscordBotFanatic.Models.Enums {
    [Flags]
    public enum Permissions {
        None=0,
        EventManager,
        CompetitionManager
    }
}