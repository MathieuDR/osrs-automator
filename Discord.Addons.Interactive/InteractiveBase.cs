using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive.Criteria;
using Discord.Addons.Interactive.Paginator;
using Discord.Addons.Interactive.Results;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive {
    public abstract class InteractiveBase : InteractiveBase<SocketCommandContext> { }

    public abstract class InteractiveBase<T> : ModuleBase<T>
        where T : SocketCommandContext {
        public InteractiveService Interactive { get; set; }

        public Task<SocketMessage> NextMessageAsync(ICriterion<SocketMessage> criterion, TimeSpan? timeout = null,
            CancellationToken token = default) {
            return Interactive.NextMessageAsync(Context, criterion, timeout, token);
        }

        public Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null,
            CancellationToken token = default) {
            return Interactive.NextMessageAsync(Context, fromSourceUser, inSourceChannel, timeout, token);
        }

        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTts = false, Embed embed = null, TimeSpan? timeout = null,
            RequestOptions options = null) {
            return Interactive.ReplyAndDeleteAsync(Context, content, isTts, embed, timeout, options);
        }

        public Task<IUserMessage> PagedReplyAsync(IEnumerable<object> pages, bool fromSourceUser = true) {
            var pager = new PaginatedMessage {
                Pages = pages
            };
            return PagedReplyAsync(pager, fromSourceUser);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, bool fromSourceUser = true) {
            var criterion = new Criteria<SocketReaction>();
            if (fromSourceUser) {
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            }

            return PagedReplyAsync(pager, criterion);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<SocketReaction> criterion) {
            return Interactive.SendPaginatedMessageAsync(Context, pager, criterion);
        }

        public RuntimeResult Ok(string reason = null) {
            return new OkResult(reason);
        }
    }
}
