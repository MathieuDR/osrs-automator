using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive.Contexts;
using FluentResults;

namespace DiscordBot.Commands.Interactive {
    public abstract class ApplicationCommand : IApplicationCommand {
        public ApplicationCommand(string name, string description) {
            Name = name.ToLowerInvariant().Trim().Replace(" ", "-");
            Description = description;
        }

        
        public string Name { get; }
        public string Description { get; }

        public abstract Task<Result> GetCommandBuilder(IServiceProvider provider);
        public abstract Task<Result> GetCommandBuilder(IServiceProvider provider, ulong guildId);

        public abstract Task<Result> HandleCommandAsync(ApplicationCommandContext context);

        public abstract Task<Result> HandleComponentAsync(MessageComponentContext context);
    }
}
