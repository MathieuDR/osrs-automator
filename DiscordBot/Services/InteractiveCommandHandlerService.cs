using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive;
using DiscordBot.Models.Contexts;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services {
    public class InteractiveCommandHandlerService {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<InteractiveCommandHandlerService> _logger;
        private readonly IServiceProvider _provider;
        private readonly ICommandStrategy _strategy;

        public InteractiveCommandHandlerService(ILogger<InteractiveCommandHandlerService> logger,
            DiscordSocketClient client,
            ICommandStrategy strategy,
            IServiceProvider provider) {
            _logger = logger;
            _client = client;
            _strategy = strategy;
            _provider = provider;

            client.InteractionCreated += OnInteraction;
        }

        public async Task SetupAsync() {
            if (_client.ConnectionState != ConnectionState.Connected) {
                _client.Connected += ClientOnConnected;
                return;
            }

            await Initialize();
        }

        private async Task ClientOnConnected() {
            _client.Connected -= ClientOnConnected;
            await Initialize();
        }

        public async Task Initialize() {
            await InitializeCommands();
        }


        private async Task OnInteraction(SocketInteraction arg) {
            if (arg is not SocketSlashCommand socketSlashCommand) {
                return;
            }
    
            var ctx = new ApplicationCommandContext(socketSlashCommand, _provider);
            var result = await _strategy.HandleApplicationCommand(ctx);
            if (result.IsFailed) {
                await ctx.RespondAsync(string.Join(", ", result.Errors));
            }
        }

        private async Task InitializeCommands() {
            var builders = await _strategy.GetCommandBuilders(false);
            var commands = await _client.Rest.GetGuildApplicationCommands(403539795944538122);

            foreach (var builder in builders) {
                await RegisterCommand(builder.Build(), commands);
            }
        }

        private async Task RegisterCommand(SlashCommandProperties command, IReadOnlyCollection<RestGuildCommand> restGlobalCommands) {
            try {
                var existing = restGlobalCommands.FirstOrDefault(x => x.Name == command.Name.Value);
                if (existing is not null) {
                    await existing.DeleteAsync();
                }

                await _client.Rest.CreateGuildCommand(command, 403539795944538122);
            } catch (ApplicationCommandException e) {
                _logger.LogWarning(e, "Cannot register command {name}", command.Name);
            } catch (Exception e) {
                _logger.LogError(e, "Error when creating command");
            }
        }
    }
}
