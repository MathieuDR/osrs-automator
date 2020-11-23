using Discord;
using Discord.Addons.Interactive;
using DiscordBotFanatic.Helpers;

namespace DiscordBotFanatic.Models.ResponseModels {
    public class CustomPaginatedMessage : PaginatedMessage {
        public CustomPaginatedMessage() {
            Options = new PaginatedAppearanceOptions {
                DisplayInformationIcon = false, JumpDisplayOptions = JumpDisplayOptions.Never
            };
        }

        public CustomPaginatedMessage(EmbedBuilder wrapper) : this() {
            EmbedWrapper = wrapper;
            this.AddPagingToFooter();
        }

        public EmbedBuilder EmbedWrapper { get; set; }
    }
}