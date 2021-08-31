using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Extensions;
using Discord.WebSocket;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.DiscordDtos;
using DiscordBot.Services.Interfaces;
using DiscordBot.Transformers;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.Output;

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

        public Task<Result<IEnumerable<Guild>>> GetAllGuilds() {
            throw new NotImplementedException();
        }

        public Task<Result> SendFailedEmbed(ulong channelId, string message, Guid traceId) {
            throw new NotImplementedException();
        }

        public Task<Result> SendWomGroupSuccessEmbed(ulong channelId, string message, int groupId, string groupName) {
            throw new NotImplementedException();
        }

        public async Task<Result> MessageLeaderboards<T>(ulong channelId, IEnumerable<MetricTypeAndPeriodLeaderboard<T>> leaderboards) where T : ILeaderboardMember {
            var channelTask = _client.GetChannelAsync(channelId);
            var metricMessages = leaderboards.Select(leaderboard => GetMessageForLeaderboard(leaderboard)).ToList();

            var toSendResult = CreateCompoundedMessagesForMultipleMessages(metricMessages);
            if (toSendResult.IsFailed) {
                return toSendResult;
            }
            
            var channel = (await channelTask).As<ISocketMessageChannel>();
            foreach (var message in toSendResult.Value) {
                await channel.SendMessageAsync(message);
            }

            return Result.Ok();
        }


        private string GetMessageForLeaderboard<T>(MetricTypeAndPeriodLeaderboard<T> leaderboard) where T : ILeaderboardMember {
            var message = $"**{leaderboard.MetricType.FriendlyName(true)}** - top gains for {leaderboard.Period}{Environment.NewLine}```";
            message += leaderboard.MembersToString(3);
            message += $"```{Environment.NewLine}";
            return message;
        }

        private Result<IEnumerable<string>> CreateCompoundedMessagesForMultipleMessages(IEnumerable<string> messages) {
            var compoundedMessages = new List<string>();
            var maxSize = 1990;
            var builder = new StringBuilder();
            foreach (var message in messages) {
                if (builder.Length + message.Length >= maxSize) {
                    if (builder.Length > 0) {
                        compoundedMessages.Add(builder.ToString());
                    }

                    builder = new StringBuilder();
                }

                builder.Append(message);
            }
            
            // Last message
            if (builder.Length > 0) {
                compoundedMessages.Add(builder.ToString());
            }

            return Result.Ok((IEnumerable<string>) compoundedMessages);
        }

        private static Stream ToStream(string image) {
            var bytes = Convert.FromBase64String(image);
            return new MemoryStream(bytes);
        }
    }
}
