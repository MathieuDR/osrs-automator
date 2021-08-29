using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive.Builders;
using DiscordBot.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive.Contexts {
    public abstract class BaseInteractiveContext<T> where T : SocketInteraction {
        public T InnerContext { get; }
        public IServiceProvider Services { get; }
        public DiscordSocketClient Client { get; }
        
        public SocketGuild Guild => Client.GetGuild(InnerContext.Channel.Cast<IGuildChannel>().GuildId);
        public bool InGuild => Guild != null;
        public SocketUser User => InnerContext.User;
        public SocketGuildUser GuildUser => InnerContext.User.Cast<SocketGuildUser>();
        public SocketTextChannel TextChannel => Channel.Cast<SocketTextChannel>();
        public SocketDMChannel DmChannel => Channel.Cast<SocketDMChannel>();
        public ISocketMessageChannel Channel => InnerContext.Channel;
        
        public InteractionReplyBuilder<T> CreateReplyBuilder(bool ephemeral = false) 
            => new InteractionReplyBuilder<T>(this).WithEphemeral(ephemeral);
        
        public Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
            => InnerContext.DeferAsync(ephemeral, options);
        
        public Task RespondAsync(
            string text = null,
            IEnumerable<Embed> embeds = null,
            bool isTts = false,
            bool ephemeral = false,
            AllowedMentions allowedMentions = null,
            RequestOptions options = null,
            MessageComponent component = null)
            => InnerContext.RespondAsync(text, embeds?.ToArray(), isTts, ephemeral, allowedMentions, options, component);
        
        public Task<RestFollowupMessage> FollowupAsync(
            string text = null,
            IEnumerable<Embed> embeds = null,
            bool isTts = false,
            bool ephemeral = false,
            AllowedMentions allowedMentions = null,
            RequestOptions options = null,
            MessageComponent component = null)
            => InnerContext.FollowupAsync(text, embeds?.ToArray(), isTts, ephemeral, allowedMentions, options, component);
        
        public EmbedBuilder CreateEmbedBuilder(string content = null)
            => new EmbedBuilder()
                .WithColor(GuildUser.GetHighestRole()?.Color ?? 0x7000FB)
                .WithDescription(content ?? string.Empty);
        
        
        protected BaseInteractiveContext(T innerContext, IServiceProvider provider)
        {
            InnerContext = innerContext;
            Services = provider;
            Client = provider.GetRequiredService<DiscordSocketClient>();
        }
    }
}
