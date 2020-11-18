using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;

namespace DiscordBotFanatic.Paginator {
    public delegate Task PerformAction(object toDelete, int index);


    public class CustomActionsPaginatedAppearanceOptions : PaginatedAppearanceOptions {
        public IEmote DeleteEmoji = new Emoji("🗑️");
        public PerformAction Delete { get; set; }

        public IEmote SelectEmoji = new Emoji("👍");
        public PerformAction Select { get; set; }

        public CustomActionsPaginatedAppearanceOptions() {
            JumpDisplayOptions = JumpDisplayOptions.Never;
            DisplayInformationIcon = false;
        }
  

        public override async Task AddReactions(RestUserMessage message, SocketCommandContext context, int pages) {
            await base.AddReactions(message, context, pages);
            
            if(Delete != null) {
                await message.AddReactionAsync(DeleteEmoji);
            }

            if(Select != null) {
                await message.AddReactionAsync(SelectEmoji);
            }
        }
    }
}