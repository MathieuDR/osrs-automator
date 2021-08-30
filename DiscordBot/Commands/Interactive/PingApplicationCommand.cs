using System;
using System.Text;
using System.Threading.Tasks;
using Common.Extensions;
using Discord;
using DiscordBot.Helpers;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models.Contexts;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.Interactive {
    public class PingApplicationCommand : ApplicationCommand {

        public PingApplicationCommand(ILogger<PingApplicationCommand> logger) : base("ping", "Lets see if the bot is awake", logger) { }
        
        protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
            builder.AddOption("info", ApplicationCommandOptionType.String, "Some extra information", false);
            builder.AddOption("time", ApplicationCommandOptionType.Boolean, "Print the ping time in ms", false);
            return Task.FromResult(builder);
        }

        public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            Logger.LogInformation("Received command!");

            var guildUser = context.GuildUser;
            var extraInfo = context.Options["name"].As<string>();
            var printTime = (bool) context.Options["time"];
            
            var builder = new StringBuilder();
            builder.AppendLine($"Hello {guildUser.DisplayName()}.");
            if (!string.IsNullOrWhiteSpace(extraInfo)) {
                builder.AppendLine(extraInfo);
            }
            
            if (printTime) {
                var timeDifference = DateTimeOffset.Now - context.InnerContext.CreatedAt;
                builder.AppendLine($"Difference is: {timeDifference.TotalMilliseconds}ms");
            }

            await context.RespondAsync(builder.ToString());
            return Result.Ok();
        }

        public override Task<Result> HandleComponentAsync(MessageComponentContext context) {
            throw new NotImplementedException();
        }

        public override bool GlobalRegister => false;
    }
}
