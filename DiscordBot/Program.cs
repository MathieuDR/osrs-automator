using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Configuration;
using DiscordBotFanatic.Jobs;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services;
using DiscordBotFanatic.Services.interfaces;
using DiscordBotFanatic.Transformers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using WiseOldManConnector.Configuration;

namespace DiscordBotFanatic {
    class Program {
        public async Task EntryPointAsync() {
            IConfiguration config = BuildConfig();
            IServiceProvider services = ConfigureServices(config); // No using statement?
            //var schedulerTask = CreateQuartzScheduler();

            try {
                var discordTask = ConfigureDiscord(services, config);
                var schedulerTask = ConfigureScheduler(services);

                //var scheduler = await schedulerTask;
                //await scheduler.Start();
                //await ScheduleTasks(scheduler);

                await discordTask;
                await schedulerTask;

                await Task.Delay(-1);
            } catch (Exception e) {
                Log.Fatal(e, $"FATAL ERROR: ");
                //await (await schedulerTask).Shutdown();
            }
        }

        static void Main() => new Program().EntryPointAsync().GetAwaiter().GetResult();

        private async Task ConfigureScheduler(IServiceProvider services) {
            var factory = services.GetRequiredService<ISchedulerFactory>();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();
        }

        private async Task ConfigureDiscord(IServiceProvider services, IConfiguration config) {
            DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            await ((CommandHandlingService) services.GetRequiredService(typeof(CommandHandlingService)))
                .InitializeAsync(services);

            var botConfig = config.GetSection("Bot").Get<BotConfiguration>();
            await client.LoginAsync(TokenType.Bot, botConfig.Token);
            await client.StartAsync();
        }


        //private async Task<IScheduler> CreateQuartzScheduler(IConfiguration config) {
        //    // Grab the Scheduler instance from the Factory
        //    StdSchedulerFactory factory = new StdSchedulerFactory();
        //    IScheduler scheduler = await factory.GetScheduler();


        //    return scheduler;
        //}

        //private async Task ScheduleTasks(IScheduler scheduler) {
        //    // define the job and tie it to our HelloJob class
        //    IJobDetail job = JobBuilder.Create<HelloJob>()
        //        .WithIdentity("job1", "group1")
        //        .Build();

        //    // Trigger the job to run now, and then repeat every 10 seconds
        //    ITrigger trigger = TriggerBuilder.Create()
        //        .WithIdentity("trigger1", "group1")
        //        .StartNow()
        //        .WithSimpleSchedule(x => x
        //            .WithIntervalInSeconds(10)
        //            .RepeatForever())
        //        .Build();

        //    // Tell quartz to schedule the job using our trigger
        //    await scheduler.ScheduleJob(job, trigger);
        //}

        private IServiceProvider ConfigureServices(IConfiguration config) {
            StartupConfiguration configuration = config.GetSection("Startup").Get<StartupConfiguration>();
            BotConfiguration botConfiguration = config.GetSection("Bot").Get<BotConfiguration>();
            WiseOldManConfiguration manConfiguration = config.GetSection("WiseOldMan").Get<WiseOldManConfiguration>();
            MetricSynonymsConfiguration metricSynonymsConfiguration =
                config.GetSection("MetricSynonyms").Get<MetricSynonymsConfiguration>();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel
                .Debug()
                .WriteTo.RollingFile(new JsonFormatter(), "logs/osrs_bot.log")
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            return new ServiceCollection()
                // Base
                .AddSingleton<DiscordSocketClient>().AddSingleton<CommandService>().AddSingleton<CommandHandlingService>()
                // Logging
                // ReSharper disable once ObjectCreationAsStatement
                //.AddLogging(builder => builder.AddConsole(x=> new ConsoleLoggerOptions(){LogToStandardErrorThreshold = LogLevel.Information}))
                .AddSingleton<ILogService, SerilogService>()
                .AddLogging(loginBuilder => loginBuilder.AddSerilog(dispose: true))
                // Extra
                .AddSingleton(config).AddSingleton(botConfiguration)
                .AddSingleton(botConfiguration.Messages)
                .AddSingleton(manConfiguration)
                .AddSingleton(metricSynonymsConfiguration)
                .AddSingleton<InteractiveService>()
                //.AddTransient<HighscoreService>()
                .AddTransient<IDiscordBotRepository>(x => new LiteDbRepository(configuration.DatabaseFile))
                .AddTransient<IPlayerService, PlayerService>()
                .AddTransient<IGroupService, GroupService>()
                .AddTransient<ICompetitionService, CompetitionService>()
                .AddTransient<IAuthenticationService, AuthenticationService>()
                .AddTransient<IOsrsHighscoreService, WiseOldManConnectorService>()
                //.AddTransient<IOsrsHighscoreService, HighscoreService>()
                //Omage services
                //.AddTransient<IImageService<MetricInfo>, MetricImageService>()
                //.AddTransient<IImageService<DeltaInfo>, DeltaImageService>()
                //.AddTransient<IImageService<RecordInfo>, RecordImageService>()
                // Add additional services here
                .AddWiseOldManApi()
                .ConfigureQuartz(config)
                .ConfigureAutoMapper()
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