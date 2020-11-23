using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.ResponseModels;

namespace DiscordBotFanatic.Helpers {
    public static class DiscordHelper {
        public static void AddEmptyField(this EmbedBuilder builder, bool inline = false) {
            builder.AddField("\u200B", "\u200B", inline);
        }

        public static CustomPaginatedMessage AddPagingToFooter(this CustomPaginatedMessage message) {
            message.EmbedWrapper.Footer ??= new EmbedFooterBuilder();
            string whitespace = " ";
            if (string.IsNullOrWhiteSpace(message.EmbedWrapper.Footer.Text)) {
                whitespace = "";
            }

            message.EmbedWrapper.Footer.Text += $"{whitespace}{{0}}/{{1}} Pages.";
            return message;
        }
    }
}