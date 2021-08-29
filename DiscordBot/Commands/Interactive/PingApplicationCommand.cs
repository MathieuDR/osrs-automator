using System;
using System.Threading.Tasks;
using DiscordBot.Commands.Interactive.Contexts;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.Interactive {
    public class PingApplicationCommand : IApplicationCommand {
        private readonly ILogger _logger;

        public PingApplicationCommand(ILogger<PingApplicationCommand> logger) {
            _logger = logger;
        }
        
        // public async Task<Result> HandleCommand(SocketSlashCommand command) {
        //     _logger.LogInformation("Received command!");
        //     
        //     var guildUser = (SocketGuildUser) command.User;
        //     var extraInfo = (string)command.Data.Options?.FirstOrDefault(x => x.Name == "info")?.Value;
        //
        //     var printTime = (bool?)command.Data.Options?.FirstOrDefault(x => x.Name == "time")?.Value ?? false;
        //
        //     var builder = new StringBuilder();
        //     builder.AppendLine($"Hello {guildUser.DisplayName()}.");
        //     if (!string.IsNullOrWhiteSpace(extraInfo)) {
        //         builder.AppendLine(extraInfo);
        //     }
        //
        //     if (printTime) {
        //         var timeDifference = DateTimeOffset.Now - command.CreatedAt;
        //         builder.AppendLine($"Difference is: {timeDifference.TotalMilliseconds}ms");
        //     }
        //
        //     await command.RespondAsync(builder.ToString());
        //     return Result.Ok();
        // }

        public string Name { get; }
        public string Description { get; }
        public Task<Result> GetCommandBuilder(IServiceProvider provider) {
            throw new NotImplementedException();
        }

        public Task<Result> GetCommandBuilder(IServiceProvider provider, ulong guildId) {
            throw new NotImplementedException();
        }

        public Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            throw new NotImplementedException();
        }

        public Task<Result> HandleComponentAsync(MessageComponentContext context) {
            throw new NotImplementedException();
        }
    }
}
