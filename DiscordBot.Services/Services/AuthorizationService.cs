using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Repository;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

internal class AuthorizationService : BaseGuildConfigurationService, IAuthorizationService {
    public AuthorizationService(ILogger<AuthorizationService> logger, IRepositoryStrategy repositoryStrategy) : base(logger, repositoryStrategy) { }


    public Task<Result> AddUserToRole(GuildUser guildUser, AuthorizationRoles roleToAdd) {
        return UpdateAuthorization(new[] { guildUser }, roleToAdd);
    }

    public Task<Result> AddDiscordRoleToRole(Role discordRole, AuthorizationRoles roleToAdd) {
        return UpdateAuthorization(new[] { discordRole }, roleToAdd);
    }

    public Task<Result> AddUsersToRole(IEnumerable<GuildUser> guildUsers, AuthorizationRoles roleToAdd) {
        return UpdateAuthorization(guildUsers, roleToAdd);
    }


    public Task<Result> AddDiscordRolesToRole(IEnumerable<Role> roles, AuthorizationRoles roleToAdd) {
        return UpdateAuthorization(roles, roleToAdd);
    }

    public Task<Result> RemoveUserToRole(GuildUser guildUser, AuthorizationRoles roleToDelete) {
        return UpdateAuthorization(new[] { guildUser }, roleToDelete, true);
    }

    public Task<Result> RemoveDiscordRoleToRole(Role discordRole, AuthorizationRoles roleToDelete) {
        return UpdateAuthorization(new[] { discordRole }, roleToDelete, true);
    }

    public Task<Result> RemoveUsersToRole(IEnumerable<GuildUser> guildUsers, AuthorizationRoles roleToDelete) {
        return UpdateAuthorization(guildUsers, roleToDelete, true);
    }


    public Task<Result> RemoveDiscordRolesToRole(IEnumerable<Role> roles, AuthorizationRoles roleToDelete) {
        return UpdateAuthorization(roles, roleToDelete, true);
    }


    public ValueTask<Result<CommandRoleConfig>> ViewConfig(Guild guild) {
        return new ValueTask<Result<CommandRoleConfig>>(Result.Ok(GetGuildConfig(guild.GuildId).CommandRoleConfig));
    }

    private Task<Result> UpdateAuthorization<T>(IEnumerable<T> entities, AuthorizationRoles role, bool removing = false) where T : GuildEntity {
        var entityArr = entities.ToArray();
        if (!entityArr.Any()) {
            return Task.FromResult(Result.Fail("No entities to add"));
        }

        var config = GetGuildConfig(entityArr.First().GuildId);

        // Temporary fix for that WOM is tied to the bot
        if (config == null) {
            return Task.FromResult(Result.Fail("No configuration found for this guild. Please set up WOM"));
        }

        UpdateCommandConfigRole(entityArr, role, config.CommandRoleConfig, removing);

        return Task.FromResult(SaveGuildConfig(config));
    }

    private static void UpdateCommandConfigRole<T>(IEnumerable<T> entities, AuthorizationRoles roleToUpdate, CommandRoleConfig config, bool removing)
        where T : GuildEntity {
        foreach (var entity in entities) {
            switch (entity) {
                case Role role:
                    UpdateRoleInAuthDictionary(config.RoleIds, role.Id, roleToUpdate, removing);
                    break;
                case GuildUser user:
                    UpdateRoleInAuthDictionary(config.UserIds, user.Id, roleToUpdate, removing);
                    break;
                default: throw new ArgumentOutOfRangeException("");
            }
        }
    }

    private static void UpdateRoleInAuthDictionary(Dictionary<ulong, AuthorizationRoles> dictionary, ulong id, AuthorizationRoles roleToUpdate,
        bool removing) {
        if (!dictionary.TryGetValue(id, out var role)) {
            if (removing) {
                // Nothing to remove
                return;
            }

            dictionary.Add(id, roleToUpdate);
        } else {
            if (removing) {
                role &= ~roleToUpdate;
            } else {
                role |= roleToUpdate;
            }

            dictionary[id] = role;
        }
    }
}

internal abstract class BaseGuildConfigurationService : RepositoryService {
    protected BaseGuildConfigurationService(ILogger logger, IRepositoryStrategy repositoryStrategy) :
        base(logger, repositoryStrategy) { }


    protected Result SaveGuildConfig(GuildConfig guildConfig) {
        var repo = GetRepository<GuildConfigRepository>(guildConfig.GuildId);
        return repo.UpdateOrInsert(guildConfig);
    }

    protected GuildConfig GetGuildConfig(GuildUser guildUser) {
        return GetGuildConfig(guildUser.GuildId);
    }

    protected GuildConfig GetGuildConfig(Guild guild) {
        return GetGuildConfig(guild.Id);
    }

    protected GuildConfig GetGuildConfig(ulong guildId) {
        var repo = GetRepository<GuildConfigRepository>(guildId);
        return repo.GetSingle().Value;
    }
}
