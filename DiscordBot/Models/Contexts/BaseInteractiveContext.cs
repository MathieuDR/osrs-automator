using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Extensions;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Helpers.Builders;
using DiscordBot.Helpers.Extensions;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Models.Contexts {
    public abstract class BaseInteractiveContext { }

    public abstract class BaseInteractiveContext<T> : BaseInteractiveContext where T : SocketInteraction {
        protected BaseInteractiveContext(T innerContext, IServiceProvider provider) {
            InnerContext = innerContext;
            Services = provider;
            InteractiveService = provider.GetRequiredService<InteractiveService>();
            Client = provider.GetRequiredService<DiscordSocketClient>();
        }

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
        public InteractiveService InteractiveService { get; }
        
        public PageBuilder CreatePageBuilder(string description = null) {
            return new PageBuilder()
                .WithColor(GuildUser.GetHighestRole()?.Color ?? 0x7000FB)
                .WithDescription(description ?? string.Empty)
                .WithCurrentTimestamp();
        }

        public string GetDisplayNameById(ulong user) {
            return Guild.GetUser(user)?.DisplayName();
        }

        public string GetDisplayNameById(IGuildUser user) {
            return GetDisplayNameById(user.Id);
        }

        
        public StaticPaginatorBuilder GetBaseStaticPaginatorBuilder(IEnumerable<PageBuilder> pageBuilders) {
            var builder = new StaticPaginatorBuilder()
                    .WithFooter(PaginatorFooter.Users | PaginatorFooter.PageNumber)
                    .WithActionOnCancellation(ActionOnStop.DeleteInput)
                    .AddUser(InnerContext.User)
                    .WithPages(pageBuilders)
                    .AddOption(new Emoji("⏪"), PaginatorAction.SkipToStart)
                    .AddOption(new Emoji("◀"), PaginatorAction.Backward)
                    .AddOption(new Emoji("🛑"), PaginatorAction.Exit)
                    .AddOption(new Emoji("▶"), PaginatorAction.Forward)
                    .AddOption(new Emoji("⏩"), PaginatorAction.SkipToEnd);
                
                return builder;
            }
        
        public Task<InteractiveMessageResult> SendPaginator(Paginator paginator, TimeSpan? timeout = null, InteractionResponseType responseType = InteractionResponseType.ChannelMessageWithSource,
            bool ephemeral = false, Action<IUserMessage> messageAction = null, bool resetTimeoutOnInput = false, CancellationToken cancellationToken = default){
            return InteractiveService.SendPaginatorAsync(paginator, InnerContext, timeout, responseType, ephemeral, messageAction, resetTimeoutOnInput, cancellationToken);
        }

        public InteractionReplyBuilder<T> CreateReplyBuilder(bool ephemeral = false) {
            return new InteractionReplyBuilder<T>(this).WithEphemeral(ephemeral);
        }

        public Task DeferAsync(bool ephemeral = false, RequestOptions options = null) {
            return InnerContext.DeferAsync(ephemeral, options);
        }

        public Task RespondAsync(
            string text = null,
            IEnumerable<Embed> embeds = null,
            bool isTts = false,
            bool ephemeral = false,
            AllowedMentions allowedMentions = null,
            RequestOptions options = null,
            MessageComponent component = null) {
            return InnerContext.RespondAsync(text, embeds?.ToArray(), isTts, ephemeral, allowedMentions, options, component);
        }

        public Task<RestFollowupMessage> FollowupAsync(
            string text = null,
            IEnumerable<Embed> embeds = null,
            bool isTts = false,
            bool ephemeral = false,
            AllowedMentions allowedMentions = null,
            RequestOptions options = null,
            MessageComponent component = null) {
            return InnerContext.FollowupAsync(text, embeds?.ToArray(), isTts, ephemeral, allowedMentions, options, component);
        }

        public EmbedBuilder CreateEmbedBuilder(string content = null) {
            return new EmbedBuilder()
                .WithColor(GuildUser.GetHighestRole()?.Color ?? 0x7000FB)
                .WithMessageAuthorFooter(User)
                .WithDescription(content ?? string.Empty)
                .WithCurrentTimestamp();
        }
    }
}
