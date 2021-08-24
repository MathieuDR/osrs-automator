using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Common.Configuration;
using DiscordBot.Configuration;
using DiscordBot.Data;
using DiscordBot.Data.Factory;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using DiscordBot.Data.Repository.Migrations;
using DiscordBot.Services;
using DiscordBot.Services.interfaces;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using WiseOldManConnector.Configuration;
using WiseOldManConnector.Interfaces;

namespace DiscordBot {
    internal class Program {
        public async Task EntryPointAsync() {
            IConfiguration config = BuildConfig();
            IServiceProvider services = ConfigureServices(config); // No using statement?
            //var schedulerTask = CreateQuartzScheduler();

            try {
                var bot = new DiscordBot(config, services);
                await bot.Run(new CancellationToken());
                await Task.Delay(-1);
            } catch (Exception e) {
                Log.Fatal(e, $"FATAL ERROR: ");
            } finally {
                Log.CloseAndFlush();
            }
        }

        private static void Main() => new Program().EntryPointAsync().GetAwaiter().GetResult();
        
        private IServiceProvider ConfigureServices(IConfiguration config) {
            throw new NotImplementedException("Currently only supported through dashboard");
        }

        private IConfiguration BuildConfig() {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true).Build();
        }
    }
}