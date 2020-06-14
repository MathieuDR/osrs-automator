using Discord;

namespace DiscordBotFanatic.Helpers {
    public static class DiscordHelper {
        public static void AddEmptyField(this EmbedBuilder builder, bool inline = false) {
            builder.AddField("\u200B", "\u200B", inline);
        }
    }
}