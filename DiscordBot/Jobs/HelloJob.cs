using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBotFanatic.Services.interfaces;
using Quartz;
using Serilog.Events;

namespace DiscordBotFanatic.Jobs {
    public class HelloJob : IJob {
        private readonly DiscordSocketClient _discord;
        private readonly ILogService _service;

        public HelloJob(DiscordSocketClient discord, ILogService service) {
            _discord = discord;
            _service = service;
        }

        public async Task Execute(IJobExecutionContext context) {
            await _service.Log("Greetings from HelloJob!", LogEventLevel.Information, null);

            if (_discord.ConnectionState == ConnectionState.Connected) {
                await _service.Log("Connected", LogEventLevel.Information, null);
                var guild = _discord.Guilds.FirstOrDefault(g => g.Name.Contains("Saskora"));

                if (guild != null) {
                    var channel = guild.Channels.FirstOrDefault(c => c.Name.Contains("botssetup")) as ISocketMessageChannel;

                    if (channel != null) {
                        //await channel.SendMessageAsync("HELLO FROM JOB!");
                        await _service.Log("Would send message but nah", LogEventLevel.Information, null);
                    } else {
                        await _service.Log("Cannot find channel", LogEventLevel.Information, null);
                    }
                } else {
                    await _service.Log("Cannot find guild", LogEventLevel.Information, null);
                }
            }

            //await Console.Out.WriteLineAsync();
        }
    }
}