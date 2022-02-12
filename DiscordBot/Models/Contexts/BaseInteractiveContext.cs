using Common.Extensions;
using Fergun.Interactive.Pagination;
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
    }

    public T InnerContext { get; }
    public IServiceProvider ServiceProvider { get; }
    public DiscordSocketClient Client { get; }

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
        return new PageBuilder()
            .WithColor(GuildUser.GetHighestRole()?.Color ?? 0x7000FB)
            .WithDescription(description ?? string.Empty)
            .WithCurrentTimestamp();
    }
    
    public PageBuilder CreatePageBuilder(EmbedBuilder embedBuilder, string description = null) {
        return PageBuilder.FromEmbedBuilder(embedBuilder)
            .WithDescription(description ?? string.Empty);
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

    public InteractionReplyBuilder<T> CreateReplyBuilder(bool ephemeral = false) {
        return new InteractionReplyBuilder<T>(this).WithEphemeral(ephemeral);
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
        return InnerContext.FollowupAsync(text, embeds?.ToArray(), isTts, ephemeral, allowedMentions, component, options: options);
    }

    public override EmbedBuilder CreateEmbedBuilder(string title = null, string content = null) {
        return new EmbedBuilder()
            .WithColor(GuildUser.GetHighestRole()?.Color ?? 0x7000FB)
            .WithMessageAuthorFooter(User)
            .WithTitle(title)
            .WithDescription(content ?? string.Empty)
            .WithCurrentTimestamp();
    }

    public override string ToString() {
        return $"{User} in {MessageLocation}: \"{Message}\"";
    }
}