using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive.Contexts;
using FluentResults;

namespace DiscordBot.Commands.Interactive {
    public interface IApplicationCommand {
        public string Name { get; }
        public string Description { get; }
        Task<Result> GetCommandBuilder(IServiceProvider provider);
        Task<Result> GetCommandBuilder(IServiceProvider provider, ulong guildId);
        Task<Result> HandleCommandAsync(ApplicationCommandContext context);
        Task<Result> HandleComponentAsync(MessageComponentContext context);
    }
}
