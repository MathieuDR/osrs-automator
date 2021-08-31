using System;
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
            var discordClient = _serviceProvider.GetRequiredService<DiscordSocketClient>();

            var commands = strategy.GetCommandDescriptions();
            // var guilds = discordClient.Guilds.Select(g => (g.Name, g.Id));

            var embed = context.CreateEmbedBuilder().WithTitle("Select a command.")
                .WithMessageAuthorFooter(context.User);

            var commandMenu = new ComponentBuilder()
                .WithSelectMenu(new SelectMenuBuilder()
                    .WithCustomId("commands:command")
                    .WithOptions(commands.Select(c=> new SelectMenuOptionBuilder()
                        .WithLabel($"{c.Name}: {c.Description}")
                        .WithValue(c.Name)).ToList())
                    .WithPlaceholder("Choose a command"));

            // var guildOptions = guilds.Select(c => new SelectMenuOptionBuilder()
            //     .WithLabel($"{c.Name}")
            //     .WithValue(c.Id.ToString())).ToList();
            //
            // guildOptions.Insert(0,new SelectMenuOptionBuilder().WithDefault(true).WithLabel("Global command").WithValue("0"));
            //
            // var guildCommand = new ComponentBuilder()
            //     .WithSelectMenu(new SelectMenuBuilder()
            //         .WithCustomId("commands:guild")
            //         .WithOptions(guildOptions)
            //         .WithPlaceholder("Choose a guild"));

            await context.RespondAsync(embeds: new[] {embed.Build()}, component: commandMenu.Build());
            return Result.Ok();
        }

        public override Task<Result> HandleComponentAsync(MessageComponentContext context) {
            throw new System.NotImplementedException();
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

        public override bool GlobalRegister => true;
    }
}
