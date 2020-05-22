using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    
    public class InfoModule : ModuleBase<SocketCommandContext> {
        private readonly IImageService _imageService;

        public InfoModule(IImageService imageService) {
            _imageService = imageService;
        }

        [Command("info")]
        [Summary("Geeft informatie over de server")]
        public Task Info()
        {
            return ReplyAsync(
                $"Hello {Context.User.Username}, I am a bot called {Context.Client.CurrentUser.Username} written in Discord.Net 2.1.\nI'm currently in the server {Context.Guild.Name} - {Context.Guild.Description} and we're talking in the channel {Context.Channel.Name}.");
        }

        [Command("format")]
        public Task Format(int number)
        {
            return ReplyAsync($"{number.FormatNumber()}");
        }

        [Command("level")]
        public Task GetLevel(int number)
        {
            return ReplyAsync($"{number.ToLevel()}");
        }

        [Command("error")]
        public Task GetError([Remainder] string errorMessage = "")
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                throw new ArgumentException($"Wow, special error!");
            }

            throw new Exception($"bla {errorMessage}");
        }

        [Command("Draw")]
        public Task DrawImage(MetricType metric, int number) {
            if (!metric.IsSkillMetric()) {
                throw new ArgumentException($"Only skills for now");
            }

            var image = _imageService.DrawSkillImage(metric, number);
            return Context.Channel.SendFileAsync(image.Stream, $"{metric.ToString()} lvl{number}.png");
        }

        [Command("DrawMetrics")]
        public async Task DrawImage(int number) {
            var metrisc = MetricHelper.GetSkillMetrics();
            foreach (var loopingMetric in metrisc)
            {
                Image image = _imageService.DrawSkillImage(loopingMetric, number);
                await Context.Channel.SendFileAsync(image.Stream, $"{loopingMetric.ToString()} lvl{number}.png");
            }
        }

        [Command("DrawSkills")]
        public async Task DrawImage(MetricType metric) {
            var metrisc = MetricHelper.GetSkillMetrics();
            for (var index = 1; index <= 99; index++) {
               
                Image image = _imageService.DrawSkillImage(metric, index);
                await Context.Channel.SendFileAsync(image.Stream, $"{metric.ToString()} lvl{index}.png");
            }
        }
    }
}