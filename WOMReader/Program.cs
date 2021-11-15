using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Serilog.Formatting.Json;
using WiseOldManConnector.Configuration;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WOMReader;

public class AggregratedDelta {
    public int Id { get; set; }
    public string Name { get; set; }
    public Dictionary<MetricType, double> Gained { get; } = new();
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public TimeSpan Duration => End - Start;
}

internal class Program {
    public async Task EntryPointAsync() {
        var config = BuildConfig();
        var services = ConfigureServices(config);

        var metrics = new List<MetricType> {
            MetricType.Overall
        };


        var logger = services.GetRequiredService<ILogger<Program>>();
        var groupApi = services.GetRequiredService<IWiseOldManGroupApi>();
        var compApi = services.GetRequiredService<IWiseOldManCompetitionApi>();
        var womConfig = services.GetRequiredService<WiseOldManConfiguration>();

        using (LogContext.PushProperty("Group", womConfig.GroupId)) {
            var competitionsResponse = await groupApi.Competitions(womConfig.GroupId);
            var competitions = competitionsResponse.Data.Where(x => x.EndDate <= DateTimeOffset.Now).OrderByDescending(c => c.StartDate).ToList();

            var dict = new Dictionary<string, List<AggregratedDelta>>();

            for (var index = 0; index < competitions.Count; index++) {
                var competition = competitions[index];
                using (LogContext.PushProperty("competition",
                           new {
                               competition.Id, competition.Metric, competition.Title, competition.Duration, competition.StartDate, competition.EndDate
                           })) {
                    logger.LogInformation($"Competition {competition.Title} - {competition.Metric}");
                    logger.LogInformation(
                        $"Time {competition.StartDate} - {competition.EndDate} ({competition.Duration})");


                    foreach (var metric in metrics) {
                        var info = await compApi.View(competition.Id, metric);
                        logger.LogInformation(
                            $"{metric.ToString()} Players {info.Data.ParticipantCount}, gained {info.Data.TotalGained}");

                        for (var i = 0; i < info.Data.Participants.Count; i++) {
                            var participant = info.Data.Participants[i];

                            logger.LogInformation(
                                $"{metric.ToString()} {participant.Player.Username}, gained {participant.CompetitionDelta.Gained}");

                            double threshold = metric.Category() switch {
                                MetricTypeCategory.Skills => 10000,
                                MetricTypeCategory.Bosses => 5,
                                MetricTypeCategory.Activities => 10,
                                MetricTypeCategory.Time => 1,
                                _ => throw new ArgumentOutOfRangeException()
                            };

                            if (participant.CompetitionDelta.Gained <= threshold) {
                                logger.LogInformation(
                                    $"{metric.ToString()} {participant.Player.Username}, inactive");
                                continue;
                            }


                            if (!dict.TryGetValue(participant.Player.Username, out var list)) {
                                list = new List<AggregratedDelta>();
                                dict.Add(participant.Player.Username, list);
                            }

                            var delta = list.FirstOrDefault(x => x.Id == info.Data.Id);
                            if (delta is null) {
                                delta = new AggregratedDelta();
                                delta.End = info.Data.EndDate;
                                delta.Start = info.Data.StartDate;
                                delta.Name = info.Data.Title;
                                delta.Id = info.Data.Id;
                                list.Add(delta);
                            }

                            delta.Gained.Add(metric, participant.CompetitionDelta.Gained);
                        }
                    }

                    logger.LogInformation("Sleep one minute");
                    Thread.Sleep(60 * 1000);
                    logger.LogInformation("Waking");
                }

                logger.LogInformation("__________________________________________");
            }

            logger.LogInformation("__________________________________________");


            foreach (var kvp in dict.OrderBy(x => x.Key)) {
                logger.LogInformation("__________________________________________");

                var totalDuration = TimeSpan.Zero;
                var metricDuration = new TimeSpan[metrics.Count];
                var gained = new double[metrics.Count];

                foreach (var aggregratedDelta in kvp.Value) {
                    totalDuration = totalDuration.Add(aggregratedDelta.Duration);

                    foreach (var gainedKvp in aggregratedDelta.Gained) {
                        var i = metrics.IndexOf(gainedKvp.Key);
                        gained[i] += gainedKvp.Value;

                        metricDuration[i] = metricDuration[i].Add(aggregratedDelta.Duration);
                    }
                }

                logger.LogInformation($"{kvp.Key}, {Math.Round(totalDuration.TotalDays, 2)} days over whole active period");


                for (var i = 0; i < metrics.Count; i++) {
                    var metric = metrics[i];
                    if (gained[i] > 0) {
                        logger.LogInformation(
                            $"{metric}, TTL: {Math.Round(gained[i], 2)}, AVG: {Math.Round(gained[i] / metricDuration[i].TotalDays, 2)} for {Math.Round(metricDuration[i].TotalDays, 2)} days");
                    } else {
                        logger.LogInformation(
                            $"{metric}, inactive");
                    }
                }
            }

            logger.LogInformation("Press enter to exit");
            Console.ReadLine();
        }
    }

    private static void Main() {
        new Program().EntryPointAsync().GetAwaiter().GetResult();
    }

    private IServiceProvider ConfigureServices(IConfiguration config) {
        var manConfiguration = config.GetSection("WiseOldMan").Get<WiseOldManConfiguration>();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel
            .Debug()
            .WriteTo.File(new JsonFormatter(), "logs/osrs_console.log")
            .WriteTo.Console(LogEventLevel.Information)
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
            .AddJsonFile($"appsettings.{environmentName}.json", true).Build();
    }
}
