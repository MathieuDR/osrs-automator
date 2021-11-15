using DiscordBot.Commands.Interactive;
using DiscordBot.Configuration;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace DiscordBot;

public class DiscordBot : BackgroundService {
    private readonly IConfiguration _config;
    private readonly ILogger<DiscordBot> _logger;
    private readonly IServiceProvider _services;
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
        await ((CommandHandlingService)_services.GetRequiredService(typeof(CommandHandlingService)))
            .InitializeAsync(_services);

        await ((InteractiveCommandHandlerService)_services.GetRequiredService(typeof(InteractiveCommandHandlerService)))
            .SetupAsync();

        var botConfig = _config.GetSection("Bot").Get<BotConfiguration>();


        //_client.InteractionCreated += ClientOnInteractionCreated;
        await _client.LoginAsync(TokenType.Bot, botConfig.Token);
        await _client.StartAsync();
    }

    private async Task ClientOnInteractionCreated(SocketInteraction arg) {
        if (arg is SocketSlashCommand command) {
            // Let's add a switch statement for the command name so we can handle multiple commands in one event.
            switch (command.Data.Name) {
                case "ping":
                    var handler = _services.GetRequiredService<PingApplicationCommandHandler>();
                    await handler.HandleCommandAsync(new ApplicationCommandContext(command, _services));
                    break;
            }
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        return Run(stoppingToken);
    }
}
