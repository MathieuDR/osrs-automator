using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive.Paginator;
using Discord.Commands;
using Discord.Rest;

namespace DiscordBot.Paginator {
    public delegate Task PerformAction(object toDelete, int index);


    public class CustomActionsPaginatedAppearanceOptions : PaginatedAppearanceOptions {
        public IEmote DeleteEmoji = new Emoji("🗑️");

        public IEmote SelectEmoji = new Emoji("👍");

        public IDictionary<IEmote, PerformAction> EmojiActions = new Dictionary<IEmote, PerformAction>();

        public CustomActionsPaginatedAppearanceOptions() {
            JumpDisplayOptions = JumpDisplayOptions.Never;
            DisplayInformationIcon = false;
            Timeout = TimeSpan.FromMinutes(1);
        }

        public PerformAction Delete { get; set; }
        public PerformAction Select { get; set; }

        public bool AwaitReactionDictionary { get; set; } = true;


        public override async Task AddReactions(RestUserMessage message, SocketCommandContext context, int pages) {
            await base.AddReactions(message, context, pages);

            if (Delete != null) {
                await message.AddReactionAsync(DeleteEmoji);
            }

            if (Select != null) {
                await message.AddReactionAsync(SelectEmoji);
            }

            if (EmojiActions.Any()) {
                foreach (KeyValuePair<IEmote, PerformAction> pair in EmojiActions) {
                    _ = message.AddReactionAsync(pair.Key).ConfigureAwait(AwaitReactionDictionary);
                }
            }
        }
    }
}