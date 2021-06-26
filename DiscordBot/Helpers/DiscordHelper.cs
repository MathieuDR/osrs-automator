using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using DiscordBotFanatic.Models.ResponseModels;

namespace DiscordBotFanatic.Helpers {
    public static class DiscordHelper {
        public static void AddEmptyField(this EmbedBuilder builder, bool inline = false) {
            builder.AddField("\u200B", "\u200B", inline);
        }

        public static string DisplayName(this IGuildUser user) {
            if (user == null) {
                return "no - user";
            }

            return user.Nickname ?? user.Username;
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