using Common.Extensions;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Configuration;
using Microsoft.Extensions.Options;

namespace DiscordBot.Commands.Interactive; 

public interface ICommandStrategy {
    public Task<Result> HandleApplicationCommand(ApplicationCommandContext context);
    public Task<Result> HandleComponentCommand(MessageComponentContext context);
    public Task<SlashCommandProperties[]> GetCommandPropertiesCollection(bool allBuilders);
    public Task<SlashCommandProperties> GetCommandProperties(string applicationCommandName);
    public Task<uint> GetCommandHash(string applicationCommandName);
    public Task<Result> HandleInteractiveCommand(BaseInteractiveContext context);
    public Task<IApplicationCommandHandler> GetHandler(string applicationCommandName);
    /// <summary>
    /// Get the names and descriptions of the commands
    /// </summary>
    /// <returns>Name, Description of commands</returns>
    public IEnumerable<(string Name, string Description)> GetCommandDescriptions();
    
    public Task ResetCommandRoleConfig(ulong guildId);
}

public class CommandStrategy : ICommandStrategy {
    public CommandStrategy(IApplicationCommandHandler[] commands, IGroupService groupService, IOptions<BotTeamConfiguration> botTeamConfiguration) {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _groupService = groupService;
        _botTeamConfiguration = botTeamConfiguration;
    }

    public Dictionary<ulong, CommandRoleConfig> GuildConfigs { get; set; } = new();
        
    private readonly IApplicationCommandHandler[] _commands;
    private readonly IGroupService _groupService;
    private readonly IOptions<BotTeamConfiguration> _botTeamConfiguration;
    
    private ulong GuildId => _botTeamConfiguration.Value.GuildId;
    private ulong OwnerId => _botTeamConfiguration.Value.OwnerId;

    public Task<Result> HandleInteractiveCommand(BaseInteractiveContext context) {
        return context switch {
            MessageComponentContext messageComponentContext => HandleComponentCommand(messageComponentContext),
            ApplicationCommandContext applicationCommandContext => HandleApplicationCommand(applicationCommandContext),
            _ => Task.FromResult(Result.Fail("Could not find context type"))
        };
    }

    public Task<IApplicationCommandHandler> GetHandler(string applicationCommandName) {
        return Task.FromResult(_commands.FirstOrDefault(c => string.Equals(c.Name, applicationCommandName, StringComparison.InvariantCultureIgnoreCase)));
    }

    public async Task<Result> HandleApplicationCommand(ApplicationCommandContext context) {
        var command = _commands.FirstOrDefault(c => c.CanHandle(context));

        if (command is null) {
            return Result.Fail(new Error("Could not find proper command handler").WithMetadata("404", true)); 
        }
        
        if(!await Authorized(context, command)) {
            return Result.Fail(new Error("You are not authorized to use this command").WithMetadata("401", true));
        }

        return await command.HandleCommandAsync(context);
    }

    public async Task<Result> HandleComponentCommand(MessageComponentContext context) {
        var command = _commands.FirstOrDefault(c => c.CanHandle(context));

        if (command is null) {
            return Result.Fail(new Error("Could not find proper component handler").WithMetadata("404", true));
        }
        
        if(!await Authorized(context, command)) {
            return Result.Fail(new Error("You are not authorized to use this command").WithMetadata("401", true));
        }

        return await command.HandleComponentAsync(context);
    }

    private async Task<bool> Authorized<T>(BaseInteractiveContext<T> context, IApplicationCommandHandler applicationCommand) where T : SocketInteraction {
        var roleRequired = applicationCommand.MinimumAuthorizationRole;
        
        // Check if authorization is required
        if (roleRequired == AuthorizationRoles.None) {
            return true;
        }
        
         // Check if user is bot owner
         if (context.User.Id == OwnerId) {
             return true;
         }
         
         // Only owner can currently do this
         if(roleRequired <= AuthorizationRoles.BotModerator) {
             return false;
         }

         // Check if user is in guild  
         if (!context.InGuild) {
             return false;
         }
         
         // Check if user is in bot team
         if (context.Guild.Id == GuildId) {
             // Do special stuff. Do we actually need this? It's just a normal server you know?
             return true;
         }
         
        var config = await GetGuildConfig(context.Guild.ToGuildDto());
        
        // Check if user has special permission
        if (config.UserIds.TryGetValue(context.User.Id, out var userRole)) {
            if (userRole >= roleRequired) {
                return true;
            }
        }

        // Check if user has specific role
        var roleIds = context.GuildUser.Roles.OrderByDescending(x=>x.Position).Select(r => r.Id);
        foreach (var roleId in roleIds) {
            if (!config.RoleIds.TryGetValue(roleId, out var roleRole)) {
                continue;
            }

            if (roleRole >= roleRequired) {
                return true;
            }
        }

        return false;
    }

    private async ValueTask<CommandRoleConfig> GetGuildConfig(Guild guild) {
        if (GuildConfigs.TryGetValue(guild.Id, out var guildConfig)) {
            return guildConfig;
        }

        var guildConfigResult = await _groupService.GetCommandRoleConfig(guild);

        if (guildConfigResult.IsFailed) {
            throw new Exception(guildConfigResult.CombineMessage());
        }
        
        GuildConfigs.Add(guild.Id, guildConfigResult.Value);
        return guildConfig;
    }

    /// <summary>
    /// Get all the command builders
    /// </summary>
    /// <param name="allBuilders">Retrieve all commands, even if they're not set to 'global'</param>
    /// <returns>SlashCommandProperties in the strategy</returns>
    public async Task<SlashCommandProperties[]> GetCommandPropertiesCollection(bool allBuilders = false) {
        var commandsToRetrieve = _commands.Where(c => c.GlobalRegister || allBuilders).ToList();
            
        var tasks = new Task<SlashCommandProperties>[commandsToRetrieve.Count];
        for (var i = 0; i < commandsToRetrieve.Count; i++) {
            var command = commandsToRetrieve[i];
            var builderTask = command.GetCommandProperties();
            tasks[i] = builderTask;
        }

        return await Task.WhenAll(tasks);
    }

    public async Task<SlashCommandProperties> GetCommandProperties(string applicationCommandName) {
        var command = await GetHandler(applicationCommandName);
   
        if (command is null) {
            return null;
        }
            
        return await command.GetCommandProperties();
    }
        
    public Task<uint> GetCommandHash(string applicationCommandName) {
        var command = _commands.FirstOrDefault(c => string.Equals(c.Name, applicationCommandName, StringComparison.InvariantCultureIgnoreCase));
   
        if (command is null) {
            return null;
        }

        return command.GetCommandBuilderHash();
    }

    public IEnumerable<(string Name, string Description)> GetCommandDescriptions() {
        return _commands.Select(c => (c.Name, c.Description));
    }

    public Task ResetCommandRoleConfig(ulong guildId) {
        GuildConfigs.Remove(guildId);
        return Task.CompletedTask;
    }
}