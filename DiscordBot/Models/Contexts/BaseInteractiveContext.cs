using DiscordBot.Common.Identities;
using Fergun.Interactive.Pagination;
using MathieuDR.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Models.Contexts;

public abstract class BaseInteractiveContext {
    public abstract bool IsDeferred { get; }

    public abstract string Command { get; }
    public abstract string SubCommand { get; }
    public abstract string SubCommandGroup { get; }
    public abstract EmbedBuilder CreateEmbedBuilder(string title = null, string content = null);
}

public abstract class BaseInteractiveContext<T> : BaseInteractiveContext where T : SocketInteraction {
    private bool _isDeferred;

    protected BaseInteractiveContext(T innerContext, IServiceProvider provider) {
        InnerContext = innerContext;
        ServiceProvider = provider;
        InteractiveService = provider.GetRequiredService<InteractiveService>();
        Client = provider.GetRequiredService<DiscordSocketClient>();
        HostingFooter = ResolveHostingFooter(provider);
    }

    public T InnerContext { get; }
    public IServiceProvider ServiceProvider { get; }
    public DiscordSocketClient Client { get; }
    public string HostingFooter { get; }

    protected virtual bool ShowsHostingFooter => true;

    // A footer lookup problem (guild not yet cached during a gateway resume, a broken common DB,
    // ...) must never break command dispatch: resolve it defensively and swallow/log any failure.
    private string ResolveHostingFooter(IServiceProvider provider) {
        if (!ShowsHostingFooter) {
            return null;
        }

        try {
            if (InnerContext.Channel is IGuildChannel && Guild is SocketGuild guild) {
                var hostingService = provider.GetRequiredService<IHostingService>();
                var status = hostingService.GetStatus(guild.GetGuildId(), guild.Name);

                if (status.FooterText is not null) {
                    hostingService.RecordFooterShown(guild.GetGuildId(), status, Command);
                }

                return status.FooterText;
            }
        } catch (Exception ex) {
            var logger = provider.GetService<ILoggerFactory>()?.CreateLogger(nameof(BaseInteractiveContext));
            logger?.LogDebug(ex, "Failed to resolve the hosting footer for this interaction");
        }

        return null;
    }

    public SocketGuild Guild => Client.GetGuild(InnerContext.Channel.Cast<IGuildChannel>().GuildId);
    public bool InGuild => Guild != null;
    public SocketUser User => InnerContext.User;
    public SocketGuildUser GuildUser => InnerContext.User.Cast<SocketGuildUser>();
    public SocketTextChannel TextChannel => Channel.Cast<SocketTextChannel>();
    public SocketDMChannel DmChannel => Channel.Cast<SocketDMChannel>();
    public ISocketMessageChannel Channel => InnerContext.Channel;
    public InteractiveService InteractiveService { get; }

    public abstract string Message { get; }
    public string MessageLocation => InGuild ? $"{Channel} ({Guild})" : "Direct Message";
    
    
    /// <summary>
    ///     Is only accurate if the inner context is not used by itself
    /// </summary>
    public override bool IsDeferred => _isDeferred;

    public PageBuilder CreatePageBuilder(string description = null) {
        var builder = new PageBuilder()
            .WithColor(GuildUser.GetHighestRole()?.Color ?? 0x7000FB)
            .WithDescription(AppendHostingFooterToDescription(description ?? string.Empty))
            .WithCurrentTimestamp();

        return builder;
    }

    public PageBuilder CreatePageBuilder(EmbedBuilder embedBuilder, string description = null) {
        var builder = PageBuilder.FromEmbedBuilder(embedBuilder)
            .WithDescription(AppendHostingFooterToDescription(description ?? string.Empty));

        return builder;
    }

    // Fergun's StaticPaginatorBuilder.WithFooter(PaginatorFooter.Users | PaginatorFooter.PageNumber)
    // (see GetBaseStaticPaginatorBuilder below) overrides any per-page embed footer, so the hosting line
    // is appended as Discord subtext at the end of the page description instead of set via .WithFooter.
    private string AppendHostingFooterToDescription(string description) {
        return HostingFooter is not null ? description + "\n-# " + HostingFooter : description;
    }

    public string GetDisplayNameById(DiscordUserId user) {
        return Guild.GetUser(user.UlongValue).DisplayName();
    }

    public string GetDisplayNameById(IGuildUser user) {
        return GetDisplayNameById(user.GetUserId());
    }

    public StaticPaginatorBuilder GetBaseStaticPaginatorBuilder(IEnumerable<PageBuilder> pageBuilders) {
        var builder = new StaticPaginatorBuilder()
            .WithFooter(PaginatorFooter.Users | PaginatorFooter.PageNumber)
            .WithActionOnCancellation(ActionOnStop.DeleteInput)
            .WithActionOnTimeout(ActionOnStop.DeleteInput)
            .WithDeletion(DeletionOptions.Invalid) // Not sure what this does.
            .AddUser(InnerContext.User)
            .WithPages(pageBuilders)
            .AddOption(new Emoji("⏪"), PaginatorAction.SkipToStart)
            .AddOption(new Emoji("◀"), PaginatorAction.Backward)
            .AddOption(new Emoji("🛑"), PaginatorAction.Exit)
            .AddOption(new Emoji("▶"), PaginatorAction.Forward)
            .AddOption(new Emoji("⏩"), PaginatorAction.SkipToEnd);

        return builder;
    }

    /// <summary>
    /// Default timeout is 5 minutes
    /// </summary>
    public Task<InteractiveMessageResult> SendPaginator(Paginator paginator, TimeSpan? timeout = null,
        InteractionResponseType responseType = InteractionResponseType.ChannelMessageWithSource,
        bool ephemeral = false, Action<IUserMessage> messageAction = null, bool resetTimeoutOnInput = true,
        CancellationToken cancellationToken = default) {
        timeout ??= TimeSpan.FromMinutes(5);

        return InteractiveService.SendPaginatorAsync(paginator, InnerContext, timeout, responseType, ephemeral, messageAction,
            resetTimeoutOnInput, cancellationToken);
    }

    public IInteractionReplyBuilder<T> CreateReplyBuilder(bool ephemeral = false) {
        return new BaseInteractionReplyBuilder<T>(this).WithEphemeral(ephemeral) as IInteractionReplyBuilder<T>;
    }
    
    public InteractionPaginatorReplyBuilder<T> CreatePaginatorReplyBuilder(bool ephemeral = false) {
        return new InteractionPaginatorReplyBuilder<T>(this).WithEphemeral(ephemeral);
    }

    public Task DeferAsync(bool ephemeral = false, RequestOptions options = null) {
        _isDeferred = true;
        return InnerContext.DeferAsync(ephemeral, options);
    }

    public virtual Task RespondAsync(
        string text = null,
        IEnumerable<Embed> embeds = null,
        bool isTts = false,
        bool ephemeral = false,
        AllowedMentions allowedMentions = null,
        RequestOptions options = null,
        MessageComponent component = null) {
        _isDeferred = true;
        text = AppendHostingFooter(text, embeds);
        return InnerContext.RespondAsync(text, embeds?.ToArray(), isTts, ephemeral, allowedMentions, component, options: options);
    }

    public Task<RestFollowupMessage> FollowupAsync(
        string text = null,
        IEnumerable<Embed> embeds = null,
        bool isTts = false,
        bool ephemeral = false,
        AllowedMentions allowedMentions = null,
        RequestOptions options = null,
        MessageComponent component = null) {
        text = AppendHostingFooter(text, embeds);
        return InnerContext.FollowupAsync(text, embeds?.ToArray(), isTts, ephemeral, allowedMentions, component, options: options);
    }

    private string AppendHostingFooter(string text, IEnumerable<Embed> embeds) {
        if (HostingFooter is not null && (embeds is null || !embeds.Any()) && !string.IsNullOrEmpty(text)) {
            text += "\n-# " + HostingFooter;
        }

        return text;
    }

    public override EmbedBuilder CreateEmbedBuilder(string title = null, string content = null) {
        return new EmbedBuilder()
            .WithColor(GuildUser.GetHighestRole()?.Color ?? 0x7000FB)
            .WithMessageAuthorFooter(User, HostingFooter ?? string.Empty)
            .WithTitle(title)
            .WithDescription(content ?? string.Empty)
            .WithCurrentTimestamp();
    }

    public override string ToString() {
        return $"{User} in {MessageLocation}: \"{Message}\"";
    }
}