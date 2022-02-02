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
        
        _logger.LogInformation("Discord bot is running.");
        await Task.Delay(-1, stoppingToken);
    }

    private async Task ConfigureScheduler() {
        var factory = _services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await factory.GetScheduler();
        await scheduler.Start();
    }

    private async Task ConfigureDiscord() {
        try {
            _logger.LogInformation("Configuring discord");
            _client = _services.GetRequiredService<DiscordSocketClient>();

            await _services.GetRequiredService<InteractiveCommandHandlerService>().SetupAsync();
            var botConfig = _config.GetSection("Bot").Get<BotConfiguration>();


            _logger.LogInformation("Logging in");
            await _client.LoginAsync(TokenType.Bot, botConfig.Token);
            
            _logger.LogInformation("Starting discord");
            await _client.StartAsync();
        } catch (Exception e) {
            _logger.LogError("Something went wrong with discord", e);
            throw;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken) {
        return Run(stoppingToken);
    }
}
