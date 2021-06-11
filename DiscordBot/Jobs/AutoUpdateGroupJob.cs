using System;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.WebSocket;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Jobs {
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