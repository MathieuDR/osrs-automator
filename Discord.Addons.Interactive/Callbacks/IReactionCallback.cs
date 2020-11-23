using System;
using System.Threading.Tasks;
using Discord.Addons.Interactive.Criteria;
using Discord.Commands;
using Discord.WebSocket;

namespace Discord.Addons.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        SocketCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
