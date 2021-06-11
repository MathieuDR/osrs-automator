using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotFanatic.Models.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using WiseOldManConnector.Configuration;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WOMReader {
    class Program {
        public async Task EntryPointAsync() {
            IConfiguration config = BuildConfig();
            IServiceProvider services = ConfigureServices(config);

            
            var logger = services.GetRequiredService<ILogger<Program>>();
            var groupApi = services.GetRequiredService<IWiseOldManGroupApi>();
            var compApi = services.GetRequiredService<IWiseOldManCompetitionApi>();
            var womConfig = services.GetRequiredService<WiseOldManConfiguration>();

            var competitionsResponse = await groupApi.Competitions(womConfig.GroupId);
            var competitions = competitionsResponse.Data.OrderByDescending(c=>c.StartDate).ToList();

            foreach (var competition in competitions) {
                logger.LogInformation($"Competition {competition.Title} - {competition.Metric}");
                logger.LogInformation($"Time {competition.StartDate} - {competition.EndDate} ({competition.Duration})");
                logger.LogInformation("_____________________");

                var ehb = await compApi.View(competition.Id, MetricType.EffectiveHoursBossing);
                ConnectorResponse<Competition> ehp = await compApi.View(competition.Id, MetricType.EffectiveHoursPlaying);
                
                logger.LogInformation($"EHB Players {ehb.Data.ParticipantCount}, gained {ehb.Data.TotalGained}");
                logger.LogInformation($"EHP Players {ehp.Data.ParticipantCount}, gained {ehp.Data.TotalGained}");
            }
            
            logger.LogInformation("Press enter to exit");
            Console.ReadLine();
        }
        
        private static void Main() => new Program().EntryPointAsync().GetAwaiter().GetResult();
        
         private IServiceProvider ConfigureServices(IConfiguration config) {
            
            WiseOldManConfiguration manConfiguration = config.GetSection("WiseOldMan").Get<WiseOldManConfiguration>();
        
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel
                .Debug()
                .WriteTo.File(new JsonFormatter(), "logs/osrs_console.log")
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            return new ServiceCollection()
                .AddLogging(loginBuilder => loginBuilder.AddSerilog(dispose: true))
                .AddSingleton(manConfiguration)
                .AddWiseOldManApi()
                .BuildServiceProvider();
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