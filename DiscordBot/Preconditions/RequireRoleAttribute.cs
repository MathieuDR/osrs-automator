using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.Preconditions; 

public class RequireRoleAttribute : PreconditionAttribute {
    private readonly List<ulong> _ids = new();

    public RequireRoleAttribute(ulong id) {
        _ids.Add(id);
    }

    public RequireRoleAttribute(ulong[] ids) {
        _ids.AddRange(ids);
    }

    private string _idsConcatenated => string.Join(", ", _ids);

    // Override the CheckPermissions method
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
        IServiceProvider services) {
        // Check if this user is a Guild User, which is the only context where roles exist
        if (!(context.User is SocketGuildUser gUser)) {
            return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }

        // If this command was executed by a user with the appropriate role, return a success
        if (gUser.Roles.Any(r => _ids.Contains(r.Id))) {
            return Task.FromResult(PreconditionResult.FromSuccess());
        }

        // Since it wasn't, fail
        return Task.FromResult(
            PreconditionResult.FromError($"You must have a role with an id in {_idsConcatenated} to run this command."));
    }
}