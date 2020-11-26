using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Interactive.Criteria;
using Discord.Addons.Interactive.Paginator;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Models.ResponseModels;

namespace DiscordBotFanatic.Paginator {
    public class CustomPaginatedMessageCallback : PaginatedMessageCallback {
        private readonly string _footerBuilderTemplate;

        public CustomPaginatedMessageCallback(InteractiveService interactive, SocketCommandContext sourceContext,
            PaginatedMessage pager, ICriterion<SocketReaction> criterion = null) : base(interactive, sourceContext, pager,
            criterion) {
            if (Pager is CustomPaginatedMessage customPaginated) {
                _footerBuilderTemplate = customPaginated.EmbedWrapper.Footer.Text;
            }
        }

        public override async Task<bool> HandleCallbackAsync(SocketReaction reaction) {
            var emote = reaction.Emote;

            // Check if its custom options
            if (!(Options is CustomActionsPaginatedAppearanceOptions customActionsOptions)) {
                return await base.HandleCallbackAsync(reaction);
            }

            // Calculate index
            var index = Page - 1;
            try {
                if (emote.Equals(customActionsOptions.SelectEmoji)) {
                    await customActionsOptions.Select(Pager.Pages.ElementAt(index), index);
                }

                if (emote.Equals(customActionsOptions.DeleteEmoji)) {
                    await customActionsOptions.Delete(Pager.Pages.ElementAt(index), index);
                    var tempList = Pager.Pages.ToList();
                    tempList.RemoveAt(Page - 1);
                    Pager.Pages = tempList;

                    if (Pager.Pages.Any()) {
                        Page = Math.Max(Page - 1, 1);
                        Pages = Pager.Pages.Count();
                    } else {
                        await Message.DeleteAsync().ConfigureAwait(false);
                        return true;
                    }
                }
            } catch (Exception e) {
                await Message.ModifyAsync(m => {
                    m.Content = $"Something went wrong with handling this reaction!\n`{e.Message}`";
                    m.Embed = null;
                });
                await Message.RemoveAllReactionsAsync().ConfigureAwait(false);
                return true;
            }

            // Go to base to delete reaction or it's none of ours.
            return await base.HandleCallbackAsync(reaction);
        }
    }
}