using Common.Extensions;
using DiscordBot.Commands.Interactive;
using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Configuration;
using Microsoft.Extensions.Options;

namespace DiscordBot.Services;

public class CommandAuthorizationService : ICommandAuthorizationService {
    private readonly ILogger<CommandAuthorizationService> _logger;
    private readonly IGroupService _groupService;
    private readonly IOptions<BotTeamConfiguration> _botTeamConfiguration;

    public CommandAuthorizationService(ILogger<CommandAuthorizationService> logger, IGroupService groupService,  IOptions<BotTeamConfiguration> botTeamConfiguration) {
        _logger = logger;
        _groupService = groupService;
        _botTeamConfiguration = botTeamConfiguration;
    }
    
    private DiscordGuildId GuildId => _botTeamConfiguration.Value.GuildId;
    private DiscordUserId OwnerId => _botTeamConfiguration.Value.OwnerId;
    
    private Dictionary<DiscordGuildId, (CommandRoleConfig config, DateTime stored)> GuildConfigs { get; set; } = new();
	
	public async ValueTask<bool> IsAuthorized<T>(ICommandRequest<BaseInteractiveContext<T>> request, BaseInteractiveContext<T> context) 
        where T : SocketInteraction 
        => await CheckAuthorization(request.MinimumAuthorizationRole, request.GetType().Name, context);
  
    public async ValueTask<bool> IsAuthorized<T>(BaseInteractiveContext<T> context, IApplicationCommandHandler applicationCommand) 
        where T : SocketInteraction 
        => await CheckAuthorization( applicationCommand.MinimumAuthorizationRole, applicationCommand.Name, context);

    private async ValueTask<bool> CheckAuthorization<T>(AuthorizationRoles roleRequired, string commandName, BaseInteractiveContext<T> context) where T : SocketInteraction {
        // Check if authorization is required
        if (roleRequired == AuthorizationRoles.None) {
            _logger.LogInformation("No authorization needed for {commandName}", commandName);
            return true;
        }

        // Check if user is bot owner
        if (context.User.GetUserId() == OwnerId) {
            _logger.LogInformation("User is bot owner, executing {commandName}", commandName);
            return true;
        }

        // Only owner can currently do this
        if (roleRequired <= AuthorizationRoles.BotModerator) {
            _logger.LogInformation("User is not bot owner, not executing {commandName}", commandName);
            return false;
        }

        // Check if user is in guild  
        if (!context.InGuild) {
            _logger.LogInformation("User is not in guild, not executing {commandName}", commandName);
            return false;
        }

        // Check if user is in bot team
        // if (context.Guild.Id == GuildId) {
        //     // Do special stuff. Do we actually need this? It's just a normal server you know?
        //     _logger.LogInformation("User is in bot team guild, not executing {commandName}", applicationCommand.Name);
        //     return false;
        // }

        // Check if user is server owner
        if (context.Guild.OwnerId == context.User.Id) {
            if (AuthorizationRoles.ClanOwner <= roleRequired) {
                _logger.LogInformation("User is server owner and has permission to execute {commandName}", commandName);
                return true;
            }
        }

        var config = await GetGuildConfig(context.Guild.ToGuildDto());
        // Check if user has special permission
        if (config.UserIds.TryGetValue(context.User.GetUserId(), out var userRole)) {
            if (userRole <= roleRequired) {
                _logger.LogInformation("User has special permission to execute {commandName}", commandName);
                return true;
            }
        }

        // Check if user has specific role
        var roleIds = context.GuildUser.Roles.OrderByDescending(x => x.Position).Select(r => r.GetRoleId());
        foreach (var roleId in roleIds) {
            if (!config.RoleIds.TryGetValue(roleId, out var roleRole)) {
                continue;
            }

            if (roleRole <= roleRequired) {
                _logger.LogInformation("User has role permission to execute {commandName}", commandName);
                return true;
            }
        }

        _logger.LogInformation("User is not authorized to execute {commandName}", commandName);
        return false;
    }

    private async ValueTask<CommandRoleConfig> GetGuildConfig(Guild guild) {
        if (GuildConfigs.TryGetValue(guild.Id, out var guildConfig)) {
            if(guildConfig.stored.AddHours(3) > DateTime.UtcNow) {
                return guildConfig.config;
            }
            
            // Cache is expired delete it
            GuildConfigs.Remove(guild.Id);
        }

        var guildConfigResult = await _groupService.GetCommandRoleConfig(guild);

        if (guildConfigResult.IsFailed) {
            throw new Exception(guildConfigResult.CombineMessage());
        }

        GuildConfigs.Add(guild.Id, (guildConfigResult.Value, DateTime.UtcNow));
        return guildConfigResult.Value;
    }
}