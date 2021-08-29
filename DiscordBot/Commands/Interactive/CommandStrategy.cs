using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Commands.Interactive.Contexts;
using FluentResults;

namespace DiscordBot.Commands.Interactive {
    public interface ICommandStrategy {
        public Task<Result> HandleApplicationCommand(ApplicationCommandContext context);
        public Task<SlashCommandBuilder[]> GetCommandBuilders(bool allBuilders);
        public Task<SlashCommandBuilder> GetCommandBuilder(string aplicationCommandName);
    }

    public class CommandStrategy : ICommandStrategy {
        public CommandStrategy(IApplicationCommand[] commands) {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        }


        private readonly IApplicationCommand[] _commands;
        public async Task<Result> HandleApplicationCommand(ApplicationCommandContext context) {
            var command = _commands.FirstOrDefault(c => c.CanHandle(context));

            if (command is null) {
                return Result.Fail("Could not find proper command handler"); 
            }

            return await command.HandleCommandAsync(context);
        }

        public async Task<SlashCommandBuilder[]> GetCommandBuilders(bool allBuilders = false) {
            var commandsToRetrieve = _commands.Where(c => c.GlobalRegister || allBuilders).ToList();
            
            var tasks = new Task<SlashCommandBuilder>[commandsToRetrieve.Count];
            for (var i = 0; i < commandsToRetrieve.Count; i++) {
                var command = commandsToRetrieve[i];
                var builderTask = command.GetCommandBuilder();
                tasks[i] = builderTask;
            }

            return await Task.WhenAll(tasks);
        }

        public async Task<SlashCommandBuilder> GetCommandBuilder(string applicationCommandName) {
            var command = _commands.FirstOrDefault(c => string.Equals(c.Name, applicationCommandName, StringComparison.InvariantCultureIgnoreCase));
   
            if (command is null) {
                return null;
            }
            
            return await command.GetCommandBuilder();
        }
    }
}
