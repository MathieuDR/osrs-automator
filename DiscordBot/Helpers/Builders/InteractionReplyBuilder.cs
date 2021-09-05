using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Extensions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models.Contexts;

namespace DiscordBot.Helpers.Builders {
    public class InteractionReplyBuilder<TInteraction> where TInteraction : SocketInteraction {
        private readonly BaseInteractiveContext<TInteraction> _context;
        private Task _updateTask;

        public InteractionReplyBuilder(BaseInteractiveContext<TInteraction> ctx) {
            _context = ctx;
        }

        public string Content { get; private set; }
        public HashSet<Embed> Embeds { get; } = new();
        public bool IsTts { get; private set; }
        public bool IsEphemeral { get; private set; }
        public AllowedMentions AllowedMentions { get; private set; } = AllowedMentions.None;
        public Task UpdateOrNoopTask => _updateTask ?? Task.CompletedTask;
        public HashSet<ActionRowBuilder> ActionRows { get; } = new();

        public InteractionReplyBuilder<TInteraction> WithContent(string content) {
            Content = content;
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithEmbed(Action<EmbedBuilder> modifier) {
            Embeds.Add(_context.CreateEmbedBuilder().Apply(modifier).Build());
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithEmbeds(params EmbedBuilder[] embedBuilders) {
            foreach (var embed in embedBuilders) {
                Embeds.Add(embed.Build());
            }

            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithEmbeds(params Embed[] embeds) {
            foreach (var embed in embeds) {
                Embeds.Add(embed);
            }

            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithEmbedFrom(StringBuilder title, StringBuilder content) {
            return WithEmbedFrom(title.ToString() ,content.ToString());
        }

        public InteractionReplyBuilder<TInteraction> WithEmbedFrom(string title, string content) {
            Embeds.Add(_context.CreateEmbedBuilder(title, content).Build());
            return this;
        }
        
        public InteractionReplyBuilder<TInteraction> WithEmbedFrom(string title, string content, Action<EmbedBuilder> embedBuilder) {
            var builder = _context.CreateEmbedBuilder(title, content);
            embedBuilder(builder);
            Embeds.Add(builder.Build());
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithTts(bool tts) {
            IsTts = tts;
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithEphemeral(bool ephemeral = true) {
            IsEphemeral = ephemeral;
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithAllowedMentions(AllowedMentions allowedMentions) {
            AllowedMentions = allowedMentions;
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithComponentMessageUpdate(Action<MessageProperties> modifier) {
            if (_context is MessageComponentContext mctx) {
                _updateTask = mctx.InnerContext.Message.ModifyAsync(modifier);
            }

            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithComponent(ComponentBuilder builder) {
            WithActionRows(builder.ActionRows);
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithActionRows(params ActionRowBuilder[] actionRows) {
            foreach (var row in actionRows) {
                ActionRows.Add(row);
            }
            
            return this;
        }

        public InteractionReplyBuilder<TInteraction> WithActionRows(IEnumerable<ActionRowBuilder> actionRows) {
            return WithActionRows(actionRows.ToArray());
        }

        public InteractionReplyBuilder<TInteraction> WithButtons(IEnumerable<ButtonBuilder> buttons) {
            return WithActionRows(buttons.Select(x => x.Build()).AsActionRow());
        }

        public InteractionReplyBuilder<TInteraction> WithButtons(params ButtonBuilder[] buttons) {
            return WithActionRows(buttons.Select(x => x.Build()).AsActionRow());
        }


        public InteractionReplyBuilder<TInteraction> WithSelectMenu(SelectMenuBuilder menu) {
            ActionRows.Add(new ActionRowBuilder().AddComponent(menu.Build()));
            return this;
        }

        public async Task RespondAsync(RequestOptions options = null) {
            await _context.RespondAsync(Content, Embeds.ToArray(), IsTts, IsEphemeral,
                AllowedMentions, options, new ComponentBuilder().AddActionRows(ActionRows).Build());
            await UpdateOrNoopTask;
        }
        
        
        // public async Task UpdateAsync(MessageFlags? flags = null,RequestOptions options = null) {
        //     await WithComponentMessageUpdate(properties => {
        //         properties.Components = new ComponentBuilder().AddActionRows(ActionRows).Build();
        //         properties.Content = Content;
        //         properties.Embeds = Embeds.ToArray();
        //         properties.Flags = flags;
        //         properties.AllowedMentions = AllowedMentions;
        //         
        //     });
        //     
        //     await _context.RespondAsync(Content, Embeds.ToArray(), IsTts, IsEphemeral,
        //         AllowedMentions, options, new ComponentBuilder().AddActionRows(ActionRows).Build());
        //     await UpdateOrNoopTask;
        // }

        public async Task<RestFollowupMessage> FollowupAsync(RequestOptions options = null) {
            var result = await _context.FollowupAsync(Content, Embeds.ToArray(), IsTts, IsEphemeral,
                AllowedMentions, options, new ComponentBuilder().AddActionRows(ActionRows).Build());
            await UpdateOrNoopTask;
            return result;
        }
    }
}
