using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive
{
    public class PaginatedMessageCallback : IReactionCallback
    {
        public SocketCommandContext Context { get; }
        public InteractiveService Interactive { get; private set; }
        public IUserMessage Message { get; private set; }

        public RunMode RunMode => RunMode.Sync;
        public ICriterion<SocketReaction> Criterion => _criterion;
        public TimeSpan? Timeout => Options.Timeout;

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
            var t = Options.AddReactions(message, Context, Pages);

            // TODO: (Next major version) timeouts need to be handled at the service-level!
            if (Timeout.HasValue && Timeout.Value != null)
            {
                _ = Task.Delay(Timeout.Value).ContinueWith(_ =>
                {
                    Interactive.RemoveReactionCallback(message);
                    _ = Message.DeleteAsync();
                });
            }
        }

        public virtual async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
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
            else if (emote.Equals(Options.Stop))
            {
                await Message.DeleteAsync().ConfigureAwait(false);
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
        
        protected virtual Embed BuildEmbed()
        {
            var builder = new EmbedBuilder()
                .WithAuthor(Pager.Author)
                .WithColor(Pager.Color)
                .WithFooter(f => f.Text = string.Format(Options.FooterFormat, Page, Pages))
                .WithTitle(Pager.Title);
            if (Pager.Pages is IEnumerable<EmbedFieldBuilder> efb)
            {
                builder.Fields = efb.Skip((Page - 1) * Options.FieldsPerPage).Take(Options.FieldsPerPage).ToList();
                builder.Description = Pager.AlternateDescription;
            } 
            else
            {
                builder.Description = Pager.Pages.ElementAt(Page - 1).ToString();
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
