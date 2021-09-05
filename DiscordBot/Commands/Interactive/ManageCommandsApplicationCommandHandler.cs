using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Helpers.Builders;
using DiscordBot.Models.Contexts;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.Interactive {
    public class ManageCommandsApplicationCommandHandler : ApplicationCommandHandler {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationCommandInfoRepository _applicationCommandInfoRepository;

        public ManageCommandsApplicationCommandHandler(ILogger<ManageCommandsApplicationCommandHandler> logger, IServiceProvider serviceProvider, IRepositoryStrategy repositoryStrategy) : base("commands",
            "Manage commands", logger) {
            _serviceProvider = serviceProvider;
            _applicationCommandInfoRepository = repositoryStrategy.CreateRepository<IApplicationCommandInfoRepository>();
        }

        public override Guid Id => Guid.Parse("FEFC7AEA-A180-4545-81C0-0010DF72258A");
        public override bool GlobalRegister => true;

        public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            var embed = context.CreateEmbedBuilder("Select a command.");
            var commandMenu = GetCommandsSelectMenu().WithButton("Cancel", SubCommand("cancel"), ButtonStyle.Danger);

            await context.RespondAsync(embeds: new[] { embed.Build() }, component: commandMenu.Build(), ephemeral: true);
            return Result.Ok();
        }

        public override async Task<Result> HandleComponentAsync(MessageComponentContext context) {
            var subCommand = context.CustomSubCommandId;
            var result = subCommand switch {
                "cancel" => await HandleCancellation(context),
                "reset" => await HandleReset(context),
                "command" => await HandleCommandSubCommand(context),
                "global" => await HandleGlobalSubCommand(context),
                "guild" => await HandleGuildSubCommand(context),
                _ => Result.Fail("Could not find subcommand handler")
            };

            if (result.IsFailed) {
                // Empty message
                await context.UpdateAsync(null, null, null, null, null);
            }

            return result;
        }

        /// <summary>
        /// Creates a guild command
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Result> HandleGuildSubCommand(MessageComponentContext context) {
            var guildId = ulong.Parse(context.SelectedMenuOptions.First());
            var command = context.EmbedFields.First(x => x.Name == "Command").Value;
            
            var commandInfo = (await _applicationCommandInfoRepository.GetByCommandName(command)).Value;
            Logger.LogInformation("{@info}", commandInfo);

            if (commandInfo is null) {
                var commandStrategy = _serviceProvider.GetRequiredService<ICommandStrategy>();
                var commandHash = await commandStrategy.GetCommandHash(command);
                commandInfo = new ApplicationCommandInfo(command) { Hash = commandHash };
            }

            var list = commandInfo.RegisteredGuilds;
            string embedDescription;
            if(!commandInfo.RegisteredGuilds.Contains(guildId)) {
                list.Add(guildId);
                embedDescription = $"Creating command: {command} for guild {guildId}";
            } else {
                list.Remove(guildId);
                embedDescription = $"Removed command: {command} for guild {guildId}";
            }

            commandInfo = commandInfo with { RegisteredGuilds = list };
            var registrationService = _serviceProvider.GetRequiredService<ICommandRegistrationService>();
            await registrationService.UpdateCommand(commandInfo);
            _applicationCommandInfoRepository.UpdateOrInsert(commandInfo);

            var embed = context.CreateEmbedBuilder("Success!", embedDescription);

            await context.UpdateAsync(embed: embed.Build(), component: null, content: null);
            return Result.Ok();
        }

        /// <summary>
        /// Creates a global command
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Result> HandleGlobalSubCommand(MessageComponentContext context) {
            var command = context.EmbedFields.First(x => x.Name == "Command").Value;
            
            var commandInfo = (await _applicationCommandInfoRepository.GetByCommandName(command)).Value;

            if (commandInfo is null) {
                var commandStrategy = _serviceProvider.GetRequiredService<ICommandStrategy>();
                var commandHash = await commandStrategy.GetCommandHash(command);
                commandInfo = new ApplicationCommandInfo(command) { Hash = commandHash };
            }

            commandInfo = commandInfo with { IsGlobal = !commandInfo.IsGlobal };
            _applicationCommandInfoRepository.UpdateOrInsert(commandInfo);
            var registrationService = _serviceProvider.GetRequiredService<ICommandRegistrationService>();
            await registrationService.UpdateCommand(commandInfo);
            
            var embed = context.CreateEmbedBuilder("Success!", $"Creating global command: {command}");

            await context.UpdateAsync(embed: embed.Build(), component: null, content: null);
            return Result.Ok();
        }

        /// <summary>
        /// Creates a guild select menu and buttons to register globally
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Result> HandleCommandSubCommand(MessageComponentContext context) {
            var command = context.SelectedMenuOptions.First();
            var commandInfo = (await _applicationCommandInfoRepository.GetByCommandName(command)).Value ?? new ApplicationCommandInfo(command);
            
            var guildSelector = GetGuildsSelectMenu(commandInfo.RegisteredGuilds)
                .WithButton("Back", SubCommand("reset"), ButtonStyle.Secondary)
                .WithButton("Cancel", SubCommand("cancel"), ButtonStyle.Danger);

            if (commandInfo.IsGlobal) {
                guildSelector.WithButton("Remove global", SubCommand("global"), ButtonStyle.Danger);
            } else {
                guildSelector.WithButton("Register globally", SubCommand("global"));
            }

            var embed = context
                .CreateEmbedBuilder("Select a guild to register or unregister.")
                .AddField("Command", command)
                .WithMessageAuthorFooter(context.User);
            await context.UpdateAsync(embeds: new[] { embed.Build() }, component: guildSelector.Build());

            return Result.Ok();
        }

        
        /// <summary>
        /// Resets the command to the start
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Result> HandleReset(MessageComponentContext context) {
            var s2 = context
                .CreateEmbedBuilder("Select a command.")
                .WithMessageAuthorFooter(context.User);
            
            var commandMenu = GetCommandsSelectMenu()
                .WithButton("Cancel", SubCommand("cancel"), ButtonStyle.Danger);
            await context.UpdateAsync(embeds: new[] { s2.Build() }, component: commandMenu.Build());

            return Result.Ok();
        }

        /// <summary>
        /// Cancels the command
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<Result> HandleCancellation(MessageComponentContext context) {
            await context.UpdateAsync("Command cancelled", null, null, null, null);
            return Result.Ok();
        }

        /// <summary>
        /// Put the commands in a select list
        /// </summary>
        /// <returns></returns>
        private ComponentBuilder GetCommandsSelectMenu() {
            var strategy = _serviceProvider.GetRequiredService<ICommandStrategy>();
            var commands = strategy.GetCommandDescriptions();
            return new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithCustomId(SubCommand("command"))
                    .WithOptions(commands.Select(c => new SelectMenuOptionBuilder()
                        .WithLabel($"{c.Name}: {c.Description}")
                        .WithValue(c.Name)).ToList())
                    .WithPlaceholder("Choose a command"));
        }

        /// <summary>
        /// Gets the select menu for the guilds
        /// </summary>
        /// <param name="registeredCommands">Guildids where the command has been registered</param>
        /// <returns></returns>
        private ComponentBuilder GetGuildsSelectMenu(IEnumerable<ulong> registeredCommands) {
            registeredCommands ??= Array.Empty<ulong>();
            var guilds = _serviceProvider.GetRequiredService<DiscordSocketClient>().Guilds
                .Select(x => (x.Name, x.Id));

            return new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithCustomId(SubCommand("guild"))
                    .WithOptions(guilds.Select(c => {
                        var label = $"{c.Name}";
                        if (registeredCommands.Contains(c.Id)) {
                            label += $" (Deregister)";
                        }
                        return new SelectMenuOptionBuilder()
                            .WithLabel(label)
                            .WithValue(c.Id.ToString());
                    }).ToList())
                    .WithPlaceholder("Choose a guild"));
        }

        protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
            return Task.FromResult(builder);
        }
    }
}
