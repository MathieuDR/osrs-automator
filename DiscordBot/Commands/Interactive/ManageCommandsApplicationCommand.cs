using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Helpers.Builders;
using DiscordBot.Models.Contexts;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Commands.Interactive {
    public class ManageCommandsApplicationCommand : ApplicationCommand{
        private readonly IServiceProvider _serviceProvider;


        public ManageCommandsApplicationCommand(ILogger<ManageCommandsApplicationCommand> logger, IServiceProvider serviceProvider) : base("commands","Manage commands", logger) {
            _serviceProvider = serviceProvider;
        }
        public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            var strategy = _serviceProvider.GetRequiredService<ICommandStrategy>();
            var embed = context.CreateEmbedBuilder().WithTitle("Select a command.")
                .WithMessageAuthorFooter(context.User);

            var commandMenu = GetCommandsSelectMenu(strategy.GetCommandDescriptions()).WithButton("Cancel", SubCommand("cancel"), ButtonStyle.Danger);


            //await context.DeferAsync();
            await context.RespondAsync(embeds: new[] {embed.Build()}, component: commandMenu.Build(), ephemeral: true);
            return Result.Ok();
        }
        
        public override async Task<Result> HandleComponentAsync(MessageComponentContext context) {
            var subCommand = context.CustomSubCommandId;

            switch (subCommand) {
                case "cancel":
                    //await context.InnerContext.Message.DeleteAsync();
                    await context.UpdateAsync("Command cancelled", null, null, null, null);
                    return Result.Ok();
                case "reset":
                    var strategy = _serviceProvider.GetRequiredService<ICommandStrategy>();
                    var s2 = context.CreateEmbedBuilder().WithTitle("Select a command.")
                        .WithMessageAuthorFooter(context.User);
                    var commandMenu = GetCommandsSelectMenu(strategy.GetCommandDescriptions())
                        .WithButton("Cancel", SubCommand("cancel"), ButtonStyle.Danger);
                    await context.UpdateAsync(embeds: new[] {s2.Build()}, component: commandMenu.Build());
                    break;
                case "command":
                    var guildSelector = GetGuildsSelectMenu().WithButton("Global", SubCommand("global"))
                        .WithButton("Back", SubCommand("reset"), ButtonStyle.Secondary)
                        .WithButton("Cancel", SubCommand("cancel"), ButtonStyle.Danger);
                    var embed = context.CreateEmbedBuilder().WithTitle("Select a guild.")
                        .AddField("Command", context.SelectedMenuOptions.First())
                        .WithMessageAuthorFooter(context.User);
                    await context.UpdateAsync(embeds: new[] {embed.Build()}, component: guildSelector.Build());
                    break;
                case "global":
                    case "guild":
                default:
                    await context.UpdateAsync(null, null, null, null, null);
                    return Result.Fail("I do not know this subcommand");
            }
            
            return Result.Ok();
        }
        
        private ComponentBuilder GetCommandsSelectMenu(IEnumerable<(string Name, string Description)> commands) => new ComponentBuilder()
            .WithSelectMenu(new SelectMenuBuilder()
                .WithCustomId(SubCommand("command"))
                .WithOptions(commands.Select(c=> new SelectMenuOptionBuilder()
                    .WithLabel($"{c.Name}: {c.Description}")
                    .WithValue(c.Name)).ToList())
                .WithPlaceholder("Choose a command"));
        
        private ComponentBuilder GetGuildsSelectMenu() {
            var guilds = _serviceProvider.GetRequiredService<DiscordSocketClient>().Guilds
                .Select(x => (x.Name, x.Id));
            
            return new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithCustomId(SubCommand("guild"))
                    .WithOptions(guilds.Select(c => new SelectMenuOptionBuilder()
                        .WithLabel($"{c.Name}")
                        .WithValue(c.Id.ToString())).ToList())
                    .WithPlaceholder("Choose a guild"));
        }

        protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
            // var commands = _strategy.GetCommandDescriptions();
            // var guilds = _client.Guilds.Select(g => (g.Name, g.Id));

            // builder.AddOption(new SlashCommandOptionBuilder()
            //     .WithName("register")
            //     .WithDescription("Register a command")
            //     .WithType(ApplicationCommandOptionType.SubCommand)
            // )
            //     .AddOption(new SlashCommandOptionBuilder()
            //         .WithName("manage")
            //         .WithDescription("manage a command")
            //         .WithType(ApplicationCommandOptionType.SubCommand)
            //     );

            return Task.FromResult(builder);
        }

        public override Guid Id => Guid.Parse("FEFC7AEA-A180-4545-81C0-0010DF72258A");
        public override bool GlobalRegister => true;
    }
}
