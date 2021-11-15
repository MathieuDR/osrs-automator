using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using FluentResults;

namespace DiscordBot.Services.Interfaces; 

public interface IAuthorizationService {
    public Task<Result> AddUserToRole(GuildUser guildUser, AuthorizationRoles roleToAdd);
    public Task<Result> AddDiscordRoleToRole(Role discordRole, AuthorizationRoles roleToAdd);
    public Task<Result> AddUsersToRole(IEnumerable<GuildUser> guildUsers, AuthorizationRoles roleToAdd);
    public Task<Result> AddDiscordRolesToRole(IEnumerable<Role> discordRoles, AuthorizationRoles roleToAdd);
    
    public Task<Result> RemoveUserToRole(GuildUser guildUser, AuthorizationRoles roleToDelete);
    public Task<Result> RemoveDiscordRoleToRole(Role discordRole, AuthorizationRoles roleToDelete);
    public Task<Result> RemoveUsersToRole(IEnumerable<GuildUser> guildUsers, AuthorizationRoles roleToDelete);
    public Task<Result> RemoveDiscordRolesToRole(IEnumerable<Role> discordRoles, AuthorizationRoles roleToDelete);

    public ValueTask<Result<CommandRoleConfig>> ViewConfig(Guild guild);
}
