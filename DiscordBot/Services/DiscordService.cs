using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.DiscordDtos;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services {
    public class DiscordService : IDiscordService {
        private readonly DiscordSocketClient _client;
        private readonly ILogger<DiscordService> _logger;

        public DiscordService(ILogger<DiscordService> logger, DiscordSocketClient client) {
            _logger = logger;
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

        public async Task<Result> PrintRunescapeDataDrop(RunescapeDropData data, ulong guildId, ulong channelId) {
            var imagesArr = data.DistinctImages.ToArray();
            
            var channel = _client.GetGuild(guildId).GetTextChannel(channelId);
            
            await channel.SendMessageAsync(
                $"New automated drop handled. Drops: {data.Drops.Count()} ({Math.Max(data.TotalValue, data.TotalHaValue)}), images: {imagesArr.Count()}");

            await channel.SendMessageAsync(
                $"drops: {string.Join(", ", data.Drops.Select(x => $"{x.Item.Name} x{x.Amount} ({Math.Max(x.TotalValue, x.TotalHaValue)})"))}");

            for (var i = 0; i < imagesArr.Length; i++) {
                var image = imagesArr[i];
                var stream = ToStream(image);
                await channel.SendFileAsync(stream, "image.png", $"Image {i}/{imagesArr.Count()}");
            }

            return Result.Ok();
        }

        private static Stream ToStream(string image) {
            var bytes = Convert.FromBase64String(image);
            return new MemoryStream(bytes);
        }
    }
}
