using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;

namespace DiscordBotFanatic.Paginator {
    public delegate Task PerformDelete(object toDelete, int index);
    public class DeletePaginatedAppearanceOptions : PaginatedAppearanceOptions {
        public IEmote Trash = new Emoji("🗑️");
        public PerformDelete Delete { get; set; }

        public override async Task AddReactions(RestUserMessage message, SocketCommandContext context, int pages) {
            if(Delete != null) {
                await base.AddReactions(message, context, pages);
            }

            await message.AddReactionAsync(Trash);
        }
    }
}