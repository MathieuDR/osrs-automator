using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
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
        public Task DrawImage(MetricType metric) {
            //if (!metric.IsSkillMetric()) {
            //    throw new ArgumentException($"Only skills for now");
            //}
            Random r = new Random();

            Metric spoofed = new Metric(){ Rank = r.Next(0,1000000), Experience = r.Next(0,14000000)};
            Tuple<MetricType, Metric> tuple = new Tuple<MetricType, Metric>(metric, spoofed);
     
            var image = _imageService.GetImageFromMetric(tuple);
            return Context.Channel.SendFileAsync(image.Stream, $"{metric.ToString()} lvl.png");
        }

        [Command("draw")]
        public Task DrawImage() {
            var metricTypes = MetricHelper.GetSkillMetrics();
            List<Tuple<MetricType, Metric>> metrics = new List<Tuple<MetricType, Metric>>();
            Random r = new Random();

            foreach (MetricType metricType in metricTypes) {
                Metric spoofed = new Metric(){ Rank = r.Next(0,1000000), Experience = r.Next(0,14000000)};
                Tuple<MetricType, Metric> tuple = new Tuple<MetricType, Metric>(metricType, spoofed);
                metrics.Add(tuple);
            }

            var image = _imageService.GetImageFromMetrics(metrics);
            return Context.Channel.SendFileAsync(image.Stream, $"allimages.png");
        }

        //[Command("DrawMetrics")]
        //public async Task DrawImage(int number) {
        //    var metrisc = MetricHelper.GetSkillMetrics();
        //    foreach (var loopingMetric in metrisc)
        //    {
        //        Image image = _imageService.DrawSkillImage(loopingMetric, number);
        //        await Context.Channel.SendFileAsync(image.Stream, $"{loopingMetric.ToString()} lvl{number}.png");
        //    }
        //}

        //[Command("DrawSkills")]
        //public async Task DrawImage(MetricType metric) {
        //    var metrisc = MetricHelper.GetSkillMetrics();
        //    for (var index = 1; index <= 99; index++) {
               
        //        Image image = _imageService.DrawSkillImage(metric, index);
        //        await Context.Channel.SendFileAsync(image.Stream, $"{metric.ToString()} lvl{index}.png");
        //    }
        //}
    }
}