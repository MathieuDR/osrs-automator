using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Services.interfaces;
using Quartz;
using Serilog.Events;

namespace DiscordBot.Jobs {
    public class HelloJob : IJob {
        private readonly DiscordSocketClient _discord;
        private readonly ILogService _service;

        public HelloJob(DiscordSocketClient discord, ILogService service) {
            _discord = discord;
            _service = service;
        }

        public async Task Execute(IJobExecutionContext context) {
            await _service.Log("Automated Hello from Hello BleuJob!", LogEventLevel.Information);

            if (_discord.ConnectionState == ConnectionState.Connected) {
                await _service.Log("Connected", LogEventLevel.Information);
                var guild = _discord.Guilds.FirstOrDefault(g => g.Name.Contains("Saskora"));

                if (guild != null) {
                    var channel = guild.Channels.FirstOrDefault(c => c.Name.Contains("botssetup")) as ISocketMessageChannel;

                    if (channel != null) {
                        //await channel.SendMessageAsync("HELLO FROM JARIGE BLOJOB!");
                        await _service.Log("Would send message but nah", LogEventLevel.Information);
                    } else {
                        await _service.Log("Cannot find channel", LogEventLevel.Information);
                    }
                } else {
                    await _service.Log("Cannot find guild", LogEventLevel.Information);
                }
            }

            //await Console.Out.WriteLineAsync();
        }
    }
}
