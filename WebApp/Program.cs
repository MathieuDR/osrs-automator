using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace WebApp {
    public class Program {
        public static async Task Main(string[] args) {
            // var dashboard = CreateHostBuilder(args).Build();
            // var bot = new DiscordBot.DiscordBot(dashboard.Services.GetRequiredService<IConfiguration>(),
            //     dashboard.Services);
            //
            // await Task.WhenAny(
            //     dashboard.RunAsync(),
            //     bot.Run());
            await CreateHostBuilder(args).Build().RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                    .ConfigureServices(((context, collection) => {
                        collection.AddHostedService<DiscordBot.DiscordBot>();
                    }))
                ;
        }
    }
}
