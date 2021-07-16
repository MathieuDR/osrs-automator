using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Addons.Interactive.Callbacks;
using Discord.Addons.Interactive.Criteria;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive.Paginator
{
    public class PaginatedMessageCallback : IReactionCallback
    {
        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Async;
        public ICriterion<SocketReaction> Criterion => _criterion;
        public TimeSpan? Timeout => Options.Timeout;

        public DateTimeOffset LastInteraction { get; set; } = DateTimeOffset.Now;

        private readonly ICriterion<SocketReaction> _criterion;
        protected readonly PaginatedMessage Pager;

        protected PaginatedAppearanceOptions Options => Pager.Options;
        protected int Pages;
        protected int Page = 1;
        

        public PaginatedMessageCallback(InteractiveService interactive, 
            SocketCommandContext sourceContext,
            PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null)
        {
            Interactive = interactive;
            Context = sourceContext;
            _criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            Pager = pager;
            Pages = Pager.Pages.Count();
            if (Pager.Pages is IEnumerable<EmbedFieldBuilder>)
                Pages = ((Pager.Pages.Count() - 1) / Options.FieldsPerPage) + 1;
        }

        public async Task DisplayAsync()
        {
            var embed = BuildEmbed();
            var message = await Context.Channel.SendMessageAsync(Pager.Content, embed: embed).ConfigureAwait(false);
            Message = message;
            Interactive.AddReactionCallback(message, this);
            // Reactions take a while to add, don't wait for them
            _ = Options.AddReactions(message, Context, Pages);

            // TODO: (Next major version) timeouts need to be handled at the service-level!
            if (Timeout != null)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ => TimeOutHandler());
            }
        }

        public virtual Task TimeOutHandler() {
            if (!Timeout.HasValue) {
                return Task.CompletedTask;
            }

            if (LastInteraction.Add(Timeout.Value) > DateTimeOffset.Now) {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ => TimeOutHandler());
                return Task.CompletedTask;
            }

            Interactive.RemoveReactionCallback(Message);
            _ = Message.RemoveAllReactionsAsync().ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public virtual async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            LastInteraction = DateTimeOffset.Now;
            
            var emote = reaction.Emote;
            _ = Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);

            if (emote.Equals(Options.First))
                Page = 1;
            else if (emote.Equals(Options.Next))
            {
                if (Page >= Pages)
                    return false;
                ++Page;
            }
            else if (emote.Equals(Options.Back))
            {
                if (Page <= 1)
                    return false;
                --Page;
            }
            else if (emote.Equals(Options.Last))
                Page = Pages;
            else if (emote.Equals(Options.Stop)) {
                Interactive.RemoveReactionCallback(Message);
                await Message.RemoveAllReactionsAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(Options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await Interactive.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > Pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await Interactive.ReplyAndDeleteAsync(Context, Options.Stop.Name);
                        return;
                    }
                    Page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(Options.Info))
            {
                await Interactive.ReplyAndDeleteAsync(Context, Options.InformationText, timeout: Options.InfoTimeout);
                return false;
            }
            
            await RenderAsync().ConfigureAwait(false);
            return false;
        }
        
        protected virtual Embed BuildEmbed() {
            var pageIndex = Page - 1;

            var builder = new EmbedBuilder()
                .WithAuthor(Pager.Author)
                .WithColor(Pager.Color)
                .WithTitle(Pager.Title);

            if (Pager.Pages is IEnumerable<EmbedFieldBuilder> efb)
            {
                builder.Fields = efb.Skip(pageIndex * Options.FieldsPerPage).Take(Options.FieldsPerPage).ToList();
                builder.Description = Pager.AlternateDescription;
            } else {
                var item = Pager.Pages.ElementAt(pageIndex);
                if (item is EmbedBuilder customBuilder) {
                    builder = customBuilder.WithFooter(f => f.Text = string.Format(Options.FooterFormat, Page, Pages));
                } else {
                    builder.Description = Pager.Pages.ElementAt(Page - 1).ToString();
                }
            }
            
            return builder.Build();
        }

        private async Task RenderAsync()
        {
            var embed = BuildEmbed();
            await Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}
