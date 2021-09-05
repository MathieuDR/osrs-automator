using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Extensions;
using Discord;
using Discord.WebSocket;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models.Contexts;
using DiscordBot.Services.Interfaces;
using DiscordBot.Transformers;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.Interactive {
    public class CountConfigurationApplicationCommandHandler: ApplicationCommandHandler {
        private readonly ICounterService _counterService;

        public CountConfigurationApplicationCommandHandler(ILogger<CountConfigurationApplicationCommandHandler> logger, ICounterService counterService) : 
            base("configuration-count", "Configure the count module", logger) {
            _counterService = counterService;
        }
        public override Guid Id => Guid.Parse("FEC9AF04-5F59-4121-95AA-F73FFCE06131");
        
        private const string SetChannelSubCommandName = "set-channel";
        private const string ChannelOption = "channel";
        private const string ViewSubCommandName = "view";
        private const string AddThresholdSubCommandName = "add-threshold";
        private const string ThresholdOption = "threshold";
        private const string NameOption = "name";
        private const string RoleOption = "role";
        public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            var subCommand = context.Options.First().Key;
            
            var result = subCommand switch {
                SetChannelSubCommandName => await SetChannelHandler(context),
                ViewSubCommandName => await ViewHandler(context),
                AddThresholdSubCommandName => await AddThresholdHandler(context),
                _ => Result.Fail("Did not find a correct handler")
            };

            return result;
        }

        private async Task<Result> AddThresholdHandler(ApplicationCommandContext context) {
            var value = (int)context.SubCommandOptions.GetOptionValue<long>(ThresholdOption);
            var role = context.SubCommandOptions.GetOptionValue<IRole>(RoleOption);
            var name = context.SubCommandOptions.GetOptionValue<string>(NameOption);
            
            try {
                if (await _counterService.CreateThreshold(context.GuildUser.ToGuildUserDto(), value, name, role?.ToRoleDto())) {
                    return Result.Fail("Could not set the channel");
                }
            } catch (Exception e) {
                return Result.Fail(new ExceptionalError(e));
            }

            await context.CreateReplyBuilder()
                .WithEmbedFrom("Success!", "Successfully created the threshold")
                .RespondAsync();
            
            return Result.Ok();
        }

        private async Task<Result> ViewHandler(ApplicationCommandContext context) {
            throw new NotImplementedException();
        }

        private async Task<Result> SetChannelHandler(ApplicationCommandContext context) {
            var channel = context.SubCommandOptions.GetOptionValue<IChannel>(ChannelOption);
            var postToChannel = context.Guild.Channels.FirstOrDefault(x => x.Id == channel.Id).As<ISocketMessageChannel>();
     
            if (postToChannel is null) {
                return Result.Fail($"The channel {channel.Name} is unavailable or not a text channel!");
            }

            try {
                if (await _counterService.SetChannelForCounts(context.GuildUser.ToGuildUserDto(), channel.ToChannelDto())) {
                    return Result.Fail("Could not set the channel");
                }
            } catch (Exception e) {
                return Result.Fail(new ExceptionalError(e));
            }

            await context.CreateReplyBuilder()
                .WithEmbedFrom("Success!", "Successfully set the channel.")
                .RespondAsync();
            return Result.Ok();
        }

        public override Task<Result> HandleComponentAsync(MessageComponentContext context) => throw new NotImplementedException();

        protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
            builder
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(SetChannelSubCommandName)
                    .WithDescription("Set the channel for threshold messages")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(ChannelOption, ApplicationCommandOptionType.Channel, "The channel to use, it must be a text channel")
                )
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(ViewSubCommandName)
                    .WithDescription("View the set channel and all thresholds")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                )
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName(AddThresholdSubCommandName)
                    .WithDescription("Adds a new threshold")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption(ThresholdOption, ApplicationCommandOptionType.Integer, "The value that triggers the threshold, inclusive")
                    .AddOption(NameOption, ApplicationCommandOptionType.String, "The name of the threshold")
                    .AddOption(RoleOption, ApplicationCommandOptionType.Role, "Optional role to give the players", false)
                );

            return Task.FromResult(builder);
        }
    }
}
