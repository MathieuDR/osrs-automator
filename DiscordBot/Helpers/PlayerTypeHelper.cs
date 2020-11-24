using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Helpers {
    public static class PlayerTypeHelper {

        public static string WiseOldManIconUrl(this PlayerType playerType) {
            string playerTypeIcon = playerType.GetEnumValueNameOrDefault();
            return $"https://wiseoldman.net/img/runescape/icons_small/{playerTypeIcon}.png";
        }
        
    }
}