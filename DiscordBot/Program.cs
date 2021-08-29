using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DiscordBot {
    internal class Program {
        public async Task EntryPointAsync() {
            var config = BuildConfig();
            var services = ConfigureServices(config); // No using statement?
            //var schedulerTask = CreateQuartzScheduler();

            try {
                var bot = new DiscordBot(config, services, services.GetRequiredService<ILogger<DiscordBot>>());
                await bot.Run(new CancellationToken());
                await Task.Delay(-1);
            } catch (Exception e) {
                Log.Fatal(e, "FATAL ERROR: ");
            }
            finally {
                Log.CloseAndFlush();
            }
        }

        private static void Main() {
            new Program().EntryPointAsync().GetAwaiter().GetResult();
        }

        private IServiceProvider ConfigureServices(IConfiguration config) {
            throw new NotImplementedException("Currently only supported through dashboard");
        }

        private IConfiguration BuildConfig() {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true).Build();
        }
    }
}
