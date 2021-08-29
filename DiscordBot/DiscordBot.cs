using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive;
using DiscordBot.Common.Configuration;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;

namespace DiscordBot {
    public class DiscordBot : BackgroundService {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;
        private readonly ILogger<DiscordBot> _logger;
        private DiscordSocketClient _client;

        public DiscordBot(IConfiguration config, IServiceProvider services, ILogger<DiscordBot> logger) {
            _config = config;
            _services = services;
            _logger = logger;
        }

        public async Task Run(CancellationToken stoppingToken) {
            var discordTask = ConfigureDiscord();
            var schedulerTask = ConfigureScheduler();

            await discordTask;
            await schedulerTask;

            await Task.Delay(-1, stoppingToken);
        }

        private async Task ConfigureScheduler() {
            var factory = _services.GetRequiredService<ISchedulerFactory>();
            var scheduler = await factory.GetScheduler();
            await scheduler.Start();
        }

        private async Task ConfigureDiscord() {
            _client = _services.GetRequiredService<DiscordSocketClient>();
            await ((CommandHandlingService) _services.GetRequiredService(typeof(CommandHandlingService)))
                .InitializeAsync(_services);

            var botConfig = _config.GetSection("Bot").Get<BotConfiguration>();

            _client.Ready += ClientOnReady;
            _client.InteractionCreated += ClientOnInteractionCreated;
                await _client.LoginAsync(TokenType.Bot, botConfig.Token);
            await _client.StartAsync();
        }

        private async Task ClientOnInteractionCreated(SocketInteraction arg) {
            if (arg is SocketSlashCommand command) {
                // Let's add a switch statement for the command name so we can handle multiple commands in one event.
                switch (command.Data.Name) {
                    case "ping":
                        var handler = _services.GetRequiredService<PingApplicationCommand>();
                        await handler.HandleCommandAsync(null);
                        break;
                }
            }
        }

        private async Task ClientOnReady() {
            await CreateSlashCommands();
        }

        private async Task CreateSlashCommands() {
            _logger.LogInformation("Creating commands");
            var commands = new List<SlashCommandProperties>();
            
            // Let's do our global command
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("ping");
            globalCommand.WithDescription("Lets see if the bot is awake");
            globalCommand.AddOption("info", ApplicationCommandOptionType.String, "Some extra information", false);
            globalCommand.AddOption("time", ApplicationCommandOptionType.Boolean, "Print the ping time in ms", false);
            commands.Add(globalCommand.Build());

            var tasks = new Task[commands.Count];
            var globals = await _client.Rest.GetGuildApplicationCommands(403539795944538122);
            for (var i = 0; i < commands.Count; i++) {
                var command = commands[i];
                tasks[i] = RegisterCommand(command, globals);
            }

            await Task.WhenAll(tasks);
        }

        private async Task RegisterCommand(SlashCommandProperties command, IReadOnlyCollection<RestGuildCommand> restGlobalCommands) {
            try {
                var existing = restGlobalCommands.FirstOrDefault(x => x.Name == command.Name.Value);
                if (existing is not null) {
                    await existing.DeleteAsync();
                }
                
                await _client.Rest.CreateGuildCommand(command,403539795944538122);
            } catch (ApplicationCommandException e) {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                //var json = JsonConvert.SerializeObject(e.Error, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                
                _logger.LogWarning(e,"Cannot register command {name}", command.Name);
            } catch (Exception e) {
                _logger.LogError(e, "Error when creating command");
            }
        }
        
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            return Run(stoppingToken);
        }
    }
}
