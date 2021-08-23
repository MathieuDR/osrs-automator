using System;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Repository;
using DiscordBot.Helpers;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.interfaces;
using DiscordBot.Services.Interfaces;

namespace DiscordBot.Jobs {
    public class AutoUpdateGroupJob : BaseGuildJob {
        private readonly IOsrsHighscoreService _osrsHighscoreService;

        public AutoUpdateGroupJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository,
            Mapper mapper, IOsrsHighscoreService osrsHighscoreService) : base(discord, logService, repository, mapper,
            JobType.GroupUpdate) {
            _osrsHighscoreService = osrsHighscoreService;
        }

        public override async Task ForGuild(SocketGuild guild, IMessageChannel channel) {
            var task = _osrsHighscoreService.UpdateGroup(Configuration.WomGroupId, Configuration.WomVerificationCode);

            var decorator = Configuration.WomGroup.Decorate();

            var builder = new EmbedBuilder()
                .AddWiseOldMan(decorator)
                .WithTimestamp(DateTimeOffset.Now);

            var update = await task;

            builder
                .WithTitle(update.IsError ? "Failure to update group." : "Group updated!")
                .WithDescription(update.Message);

            await channel.SendMessageAsync("", false, builder.Build());
        }
    }
}