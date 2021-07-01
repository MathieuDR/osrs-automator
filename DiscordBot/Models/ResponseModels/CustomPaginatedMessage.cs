using Discord;
using Discord.Addons.Interactive.Paginator;
using DiscordBot.Helpers;

namespace DiscordBot.Models.ResponseModels {
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