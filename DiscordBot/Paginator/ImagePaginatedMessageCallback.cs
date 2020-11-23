using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Models.ResponseModels;

namespace DiscordBotFanatic.Paginator {
    public class ImagePaginatedMessageCallback : PaginatedMessageCallback {
        private readonly string _footerBuilderTemplate;

        public ImagePaginatedMessageCallback(InteractiveService interactive, SocketCommandContext sourceContext,
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

            if (emote.Equals(customActionsOptions.SelectEmoji)) {
                await customActionsOptions.Select(Pager.Pages.ElementAt(index), index);
                await Message.DeleteAsync().ConfigureAwait(false);
                return true;
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

            // None of our custom options, go to base anyway.
            return await base.HandleCallbackAsync(reaction);
        }

        protected override Embed BuildEmbed() {
            if (Pager is CustomPaginatedMessage customPaginated) {
                customPaginated.EmbedWrapper.Footer.Text = string.Format(_footerBuilderTemplate, Page, Pages);

                if (Pager.Pages is IEnumerable<EmbedFieldBuilder> efb) {
                    customPaginated.EmbedWrapper.Fields =
                        efb.Skip((Page - 1) * Options.FieldsPerPage).Take(Options.FieldsPerPage).ToList();
                    customPaginated.EmbedWrapper.Description = Pager.AlternateDescription;
                } else {
                    customPaginated.EmbedWrapper.Description = Pager.Pages.ElementAt(Page - 1).ToString();
                }

                return customPaginated.EmbedWrapper.Build();
            } else {
                return base.BuildEmbed();
            }

            //var embed = base.BuildEmbed();
            //var builder = embed.ToEmbedBuilder();
            //builder.Description += "_CUSTOM";
            //return builder.Build();
        }
    }
}