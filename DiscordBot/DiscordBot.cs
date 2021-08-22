using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Models.Configuration;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace DiscordBot {
    public class DiscordBot: BackgroundService {
        private readonly IConfiguration _config;
        private readonly IServiceProvider _services;
        private DiscordSocketClient _client;
        
        public DiscordBot(IConfiguration config, IServiceProvider services) {
            _config = config;
            _services = services;
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

            await _client.LoginAsync(TokenType.Bot, botConfig.Token);
            await _client.StartAsync();
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            return Run(stoppingToken);
        }
    }
}
