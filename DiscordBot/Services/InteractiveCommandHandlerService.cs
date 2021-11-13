using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive;
using DiscordBot.Data.Interfaces;
using DiscordBot.Models.Contexts;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services {
    public class InteractiveCommandHandlerService {
        private const ulong OwnerGuildId = 403539795944538122;
        private readonly DiscordSocketClient _client;
        private readonly IApplicationCommandInfoRepository _commandInfoRepository;
        private readonly ILogger<InteractiveCommandHandlerService> _logger;
        private readonly IServiceProvider _provider;
        private readonly ICommandRegistrationService _registrationService;
        private readonly ICommandStrategy _strategy;

        public InteractiveCommandHandlerService(ILogger<InteractiveCommandHandlerService> logger,
            DiscordSocketClient client,
            ICommandStrategy strategy,
            IServiceProvider provider,
            IApplicationCommandInfoRepository commandInfoRepository,
            ICommandRegistrationService registrationService) {
            _logger = logger;
            _client = client;
            _strategy = strategy;
            _provider = provider;
            _commandInfoRepository = commandInfoRepository;
            _registrationService = registrationService;

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
            BaseInteractiveContext ctx = arg switch {
                SocketSlashCommand socketSlashCommand => new ApplicationCommandContext(socketSlashCommand, _provider),
                SocketMessageComponent socketMessageComponent => new MessageComponentContext(socketMessageComponent, _provider),
                _ => null
            };

            _logger.LogInformation("[{ctx}] Command created", ctx);
            var result = await _strategy.HandleInteractiveCommand(ctx).ConfigureAwait(false);

            if (result.IsFailed) {
                var msg = string.Join(", ",
                    result.Errors.Where(x => !x.HasMetadata("404", o => (bool)(o ?? false))).Select(x => x.Message));
                _logger.LogWarning("[{ctx}] failed: {msg}", ctx, msg);

                if (ctx is null || ctx.IsDeferred) {
                    await arg.FollowupAsync(msg);
                } else {
                    await arg.RespondAsync(msg);
                }
            }

            _logger.LogInformation("[{ctx}] done", ctx);
        }

        private async Task InitializeCommands() {
            var manageCommand = _provider.GetRequiredService<ManageCommandsApplicationCommandHandler>();
            await RegisterCommandForOwnersGuild(manageCommand);

            var commandInfos = _commandInfoRepository.GetAll().Value;
            await _registrationService.UpdateAllCommands(commandInfos);
        }

        private async Task RegisterCommandForOwnersGuild(IApplicationCommandHandler handler) {
            var propertiesTask = handler.GetCommandProperties();
            var commands = await _client.GetGuild(OwnerGuildId).GetApplicationCommandsAsync();

            try {
                var existing = commands.FirstOrDefault(x => x.Name == handler.Name && x.IsGlobalCommand == false);
                if (existing is not null) {
                    await existing.DeleteAsync();
                }

                await _client.Rest.CreateGuildCommand(await propertiesTask, OwnerGuildId);
            } catch (ApplicationCommandException e) {
                _logger.LogWarning(e, "Cannot register command {name} in the owners guild", handler.Name);
            } catch (Exception e) {
                _logger.LogError(e, "Error when creating command");
            }
        }
    }
}
