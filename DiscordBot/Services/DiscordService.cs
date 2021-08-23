using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Common.Models.DiscordDtos;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services {
    public class DiscordService : BaseService, IDiscordService {
        private readonly DiscordSocketClient _client;

        public DiscordService(ILogger<DiscordService> logger, DiscordSocketClient client) : base(logger) {
            _client = client;
        }

        public async Task<Result> SetUsername(GuildUser user, string nickname) {
            var guild = _client.GetGuild(user.GuildId);
            if (guild == null) {
                return Result.Fail($"Cannot find guild with id {user.GuildId}");
            }

            var discordUser = guild.GetUser(user.Id);
            if (discordUser == null) {
                return Result.Fail($"Cannot find user in guild with id {user.Id}");
            }

            await discordUser.ModifyAsync(x => x.Nickname = nickname);
            return Result.Ok();
        }
    }
}
