using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Services;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    [Name("Player stats")]
    public class PlayerStatsModule : StatsBaseModule {
        private const string UsernameSummary = "The OSRS Player's username";
        private readonly IImageService<MetricInfo> _metricImageService;


        public PlayerStatsModule(IImageService<MetricInfo> metricImageService, HighscoreService osrsHighscoreService, MessageConfiguration messageConfiguration) : base(osrsHighscoreService, messageConfiguration) {
            _metricImageService = metricImageService;
        }

        public string Area {
            get { return string.IsNullOrEmpty(Name) ? "Players" : Name; }
        }

        protected override void BeforeExecute(CommandInfo command) {
            if (Context != null) {
                Name = Service.GetUserNameFromUser(Context.User);
            }

            base.BeforeExecute(command);
        }

        protected override string GetUrl() {
            return WiseOldManId > 0 ? $"https://wiseoldman.net/players/{WiseOldManId}" : base.GetUrl();
        }

        #region formatting

        protected override Embed GetEmbedResponse() {
            if (Response == null) {
                return null;
            }

            switch (Response) {
                case PlayerResponse playerResponse:
                    return FormatEmbeddedFromPlayerResponse(playerResponse);
                case DeltaResponse deltaResponse:
                    return FormatEmbeddedFromDeltaResponse(deltaResponse);
                case RecordResponse recordResponse:
                    return FormatEmbeddedFromRecordResponse(recordResponse);
                default:
                    throw new Exception($"Response type unknown");
            }
        }

        private Embed FormatEmbeddedFromPlayerResponse(PlayerResponse player) {
            var embed = GetCommonEmbedBuilder(Area, $"{player.Username} stats from {player.UpdatedAt:dddd, dd/MM} at {player.UpdatedAt:HH:mm:ss}");

            //List<Tuple<MetricType, Metric>> toImage = new List<Tuple<MetricType, Metric>>();
            List<MetricInfo> infos = new List<MetricInfo>();
            foreach (KeyValuePair<string, Metric> keyValuePair in player.LatestSnapshot.MetricDictionary) {
                MetricType type = keyValuePair.Key.ToMetricType();
                //toImage.Add(new Tuple<MetricType, Metric>(type, keyValuePair.Value));
                infos.Add(keyValuePair.Value.ToMetricInfo(type));
            }

            Image image = _metricImageService.GetImages(infos);


            Context.Message.Channel.SendFileAsync(image.Stream, $"{player.Id}{player.UpdatedAt.Ticks}.png");
            return embed.Build();
        }

        private Embed FormatEmbeddedFromDeltaResponse(DeltaResponse delta) {
            var embed = GetCommonEmbedBuilder(Area, $"{Area} Gains!", $"Over a period of {delta.Interval}! Stoinks!");
            FormatMetricsInEmbed(delta.Metrics.AllDeltaMetrics, embed);

            return embed.Build();
        }

        private Embed FormatEmbeddedFromRecordResponse(RecordResponse recordResponse) {
            string periodString = "period";
            if (CommandPeriod.HasValue) {
                periodString = CommandPeriod.Value.ToString().ToLowerInvariant();
            }


            var embed = GetCommonEmbedBuilder($"{Area} Records!", $"The maximum experience earned over a {periodString}, What a beast!");

            if (!recordResponse.Records.Any(x => x.Value > 0)) {
                embed.Description = $"No records for {Area} over the period of a {periodString}";
                if (CommandMetricType.HasValue) {
                    embed.Description += $" in the metric {CommandMetricType.ToString()?.ToLowerInvariant()}";
                }

                return embed.Build();
            }

            var orderedList = recordResponse.Records.OrderBy(x => x.Period).ThenByDescending(x => x.Value).ToList();
            bool isInline = orderedList.Count() > 4;

            foreach (Record record in orderedList) {
                string title = $"{record.Period} record for {record.Metric.ToString().ToLowerInvariant()}";
                if (isInline) {
                    title = $"{record.Metric.ToEmoji()} {record.Metric.ToString().ToLowerInvariant()}";
                }

                if (record.Value > 0) {
                    try {
                        embed.AddField(title, record.ToString(), isInline);
                    } catch {
                        // ignored
                    }
                }
            }

            return embed.Build();
        }

        private void FormatMetricsInEmbed<T>(Dictionary<string, T> metricDictionary, EmbedBuilder embed) {
            bool hasRecords = false;

            foreach (KeyValuePair<string, T> kvp in metricDictionary) {
                MetricType type = Enum.Parse<MetricType>(kvp.Key, true);
                if ((CommandMetricType.HasValue && type != CommandMetricType.Value) || !type.IsSkillMetric()) {
                    continue;
                }

                //embed.Description += $"{type.ToEmoji()} {kvp.Key}: {kvp.Value}{Environment.NewLine}{Environment.NewLine}";
                //embed.Description += $"{type.ToEmoji()} {kvp.Key}{Environment.NewLine}";
                //embed.Description += $"{kvp.Value}{Environment.NewLine}";
                //embed.AddField($"{type.ToEmoji()} {kvp.Key}", kvp.Value);

                string value = kvp.Value.ToString();
                if (string.IsNullOrEmpty(value) || value == "-1") {
                    continue;
                }

                try {
                    embed.AddField($"{type.ToEmoji()} {kvp.Key}", value, true);
                    hasRecords = true;
                } catch {
                    // ignored
                }
            }

            if (hasRecords) {
                return;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append($"No records or stats for {Area}");

            if (CommandPeriod.HasValue) {
                builder.Append($" over a period of {CommandPeriod.Value}");
            }

            if (CommandMetricType.HasValue) {
                builder.Append($" in the metric {CommandMetricType.ToString()?.ToLowerInvariant()}");
            }

            builder.Append(".");
            embed.Description = builder.ToString();
        }

        #endregion


        #region player

        [Name("Get")]
        [Command("get", RunMode = RunMode.Async)]
        [Summary("Current highscores of a player, Will try to update if older then 30 minutes.")]
        public async Task GetPlayer([Remainder] MetricArguments arguments = null) {
            ExtractMetricArguments(arguments);
            Response = await GetPlayerInfo();
        }

        [Name("Delta")]
        [Command("delta", RunMode = RunMode.Async)]
        [Alias("gains", "gain")]
        [Summary("The difference of a players stats in a period of time.")]
        public async Task GetDelta([Remainder] PeriodAndMetricArguments arguments = null) {
            CommandPeriod = Period.Week;
            ExtractPeriodAndMetricArguments(arguments);
            Response = await Delta();
        }

        [Name("Records")]
        [Command("records", RunMode = RunMode.Async)]
        [Alias("record")]
        [Summary("Record gains of a specific stat and/or period.")]
        public async Task GetRecords([Remainder] PeriodAndMetricArguments arguments = null) {
            CommandPeriod = Period.Week;
            ExtractPeriodAndMetricArguments(arguments);
            Response = await GetPlayerRecord();
        }

        [Name("Update")]
        [Command("update", RunMode = RunMode.Async)]
        [Alias("new")]
        [Summary("Force refresh your stats if older then 1 minute, and display them like the `get` command")]
        public async Task UpdatePlayer([Summary(UsernameSummary)] string username = "") {
            _timeToUpdate = 1;
            if (!string.IsNullOrEmpty(username)) {
                Name = username;
            }

            Response = await GetPlayerInfo();
        }

        [Name("Set default character")]
        [Command("set", RunMode = RunMode.Async)]
        [Alias("default")]
        [Summary("Set your standard player to use within the `player` module.")]
        public async Task SetDefaultPlayer([Summary(UsernameSummary)] string username) {
            Service.SetDefaultPlayer(Context.User.Id, username);
            await WaitMessage.ModifyAsync(x => { x.Embed = GetCommonEmbedBuilder($"Done!", $"{username} is sucesfully set as your standard").Build(); });
        }

        #endregion

        #region API Wrapper

        private async Task<RecordResponse> GetPlayerRecord() {
            var playerResponse = await GetPlayerInfo();

            return await Service.GetPlayerRecordAsync(playerResponse.Id, CommandMetricType, CommandPeriod);
        }

        private async Task<PlayerResponse> GetPlayerInfo() {
            if (string.IsNullOrEmpty(Name)) {
                throw new ArgumentException($"{MessageUserDisplay} you don't have a default set. Use the `set` command.");
            }

            var response = await ShouldUpdate();

            if (response != null) {
                return response;
            }


            return await Service.TrackPlayerAsync(Name);
        }

        // Don't forget to update the GET function summary!
        private int _timeToUpdate = 30;

        private async Task<PlayerResponse> ShouldUpdate() {
            var response = await Service.GetPlayerAsync(Name);
            WiseOldManId = response.Id;
            if (response.UpdatedAt.AddMinutes(_timeToUpdate) >= DateTime.UtcNow) {
                return response;
            }

            return null;
        }

        private async Task<DeltaResponse> Delta() {
            // checks if we need to update or not, if so updates.
            var response = await GetPlayerInfo();

            Debug.Assert(CommandPeriod != null, nameof(CommandPeriod) + " != null");
            return await Service.DeltaPlayerAsync(response.Id, CommandPeriod.Value);
        }

        #endregion
    }
}