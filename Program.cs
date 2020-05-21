using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Modules;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services;
using DiscordBotFanatic.Services.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace DiscordBotFanatic
{
    class Program
    {
        static void Main()
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync() {
            IConfiguration config = BuildConfig();
            IServiceProvider services = ConfigureServices(config); // No using statement?
            //var test = services.GetService<PlayerStatsModule>();

            DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            
            await ((CommandHandlingService) services.GetRequiredService(typeof(CommandHandlingService)))
                .InitializeAsync(services);

            var botConfig = config.GetSection("Bot").Get<BotConfiguration>();
            await client.LoginAsync(TokenType.Bot, botConfig.Token);
            await client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices(IConfiguration config) {
            StartupConfiguration configuration = config.GetSection("Startup").Get<StartupConfiguration>();
            BotConfiguration botConfiguration = config.GetSection("Bot").Get<BotConfiguration>();

            return new ServiceCollection()
                // Base
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                // ReSharper disable once ObjectCreationAsStatement
                .AddLogging(builder => builder.AddConsole(x=> new ConsoleLoggerOptions(){LogToStandardErrorThreshold = LogLevel.Debug}))
                .AddSingleton<ILogService, LogService>()
                // Extra
                .AddSingleton(config)
                .AddSingleton(botConfiguration)
                .AddSingleton(botConfiguration.Messages)
                .AddTransient<WiseOldManConsumer>()
                .AddTransient<IDiscordBotRepository>(x=> new LiteDbRepository(configuration.DatabaseFile))
                .AddTransient<IGuildService, GuildService>()
                //Modules
                .AddTransient<PlayerStatsModule>()
                // Add additional services here
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
        }
    }
}
