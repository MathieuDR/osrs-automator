using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace WebApp {
    public class Program {
        public static async Task Main(string[] args) {
            Startup.CreateLogger();
            try {
                Log.Information("Starting web host");
                await CreateHostBuilder(args).Build().RunAsync();
            } catch (Exception e) {
                Log.Fatal(e, "FATAL ERROR: ");
            } finally {
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) {
            return Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
                    .ConfigureServices((context, collection) => {
                        collection.AddHostedService<DiscordBot.DiscordBot>();
                    })
                ;
        }
    }
}
