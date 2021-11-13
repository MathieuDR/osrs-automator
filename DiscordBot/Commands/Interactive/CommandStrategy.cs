using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Models.Contexts;
using FluentResults;

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
}

public class CommandStrategy : ICommandStrategy {
    public CommandStrategy(IApplicationCommandHandler[] commands) {
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
    }
        
    private readonly IApplicationCommandHandler[] _commands;

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

        return await command.HandleCommandAsync(context);
    }

    public async Task<Result> HandleComponentCommand(MessageComponentContext context) {
        var command = _commands.FirstOrDefault(c => c.CanHandle(context));

        if (command is null) {
            return Result.Fail(new Error("Could not find proper component handler").WithMetadata("404", true));
        }

        return await command.HandleComponentAsync(context);
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
}