using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Services;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    [Name("Player stats")]
    public class PlayerStatsModule : StatsBaseModule {
        private const string UsernameSummary = "The OSRS Player's username";
        private readonly IImageService<DeltaInfo> _deltaImageService;
        private readonly IImageService<MetricInfo> _metricImageService;
        private readonly IImageService<RecordInfo> _recordImageService;
        private bool _onlyPositiveDeltas;


        public PlayerStatsModule(IImageService<MetricInfo> metricImageService, IImageService<DeltaInfo> deltaImageService,  IImageService<RecordInfo> recordImageService, HighscoreService osrsHighscoreService, MessageConfiguration messageConfiguration) : base(osrsHighscoreService, messageConfiguration) {
            _metricImageService = metricImageService;
            _deltaImageService = deltaImageService;
            _recordImageService = recordImageService;
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
            Image image;

            if (CommandMetricType.HasValue) {
                var info = player.MetricForType(CommandMetricType.Value);
                image = _metricImageService.GetImage(info);
            } else {
                List<MetricInfo> infos = player.MetricInfoList;
                image = _metricImageService.GetImages(infos);
            }

            Context.Message.Channel.SendFileAsync(image.Stream, $"{player.Id}{player.UpdatedAt.Ticks}.png");

            return embed.Build();
        }

        private Embed FormatEmbeddedFromDeltaResponse(DeltaResponse delta) {
            var embed = GetCommonEmbedBuilder(Area, $"{Area} Gains!", $"Over a period of a {CommandPeriod}! Stoinks~!");

            Image image;
            if (CommandMetricType.HasValue) {
                var deltaInfo = delta.DeltaInfoForType(CommandMetricType.Value);
                if (!deltaInfo.IsRanked) {
                    throw new Exception($"Unranked! We couldn't calculate the gains.");
                }else if (_onlyPositiveDeltas && deltaInfo.Gained <= 0) {
                    throw new Exception($"You did not gain anything in this metric over the chosen period.");
                }

                image = _deltaImageService.GetImage(deltaInfo);
            } else {
                List<DeltaInfo> infos = delta.DeltaInfoList.RemoveUnrankedInfos();

                if (_onlyPositiveDeltas) {
                    infos = infos.RemoveNegativeInfos();
                }

                image = _deltaImageService.GetImages(infos);
            }

            Context.Message.Channel.SendFileAsync(image.Stream, $"delta_{Name}_{WiseOldManId}_{delta.UpdatedAt}.png");

            return embed.Build();
        }

        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        private Embed FormatEmbeddedFromRecordResponse(RecordResponse recordResponse) {
            if (!recordResponse.RecordInfos.Any()) {
                throw new Exception($"No records for this request!");
            }

            string description = recordResponse.RecordInfos.IsSamePeriod() ? $"Over the period of {CommandPeriod.Value}" : $"For {CommandMetricType.Value}";
            var embed = GetCommonEmbedBuilder(Area, $"Records for {Name}", description);

            Image image;
            if (recordResponse.RecordInfos.Count == 1) {
                var info = recordResponse.RecordInfos.FirstOrDefault();
                image = _recordImageService.GetImage(info);
            } else {
                image = _recordImageService.GetImages(recordResponse.RecordInfos);
            }

            Context.Message.Channel.SendFileAsync(image.Stream, $"records_{Name}_{WiseOldManId}_{DateTime.Now.Date}.png");

            return embed.Build();
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

        [Name("Gained")]
        [Command("Gained", RunMode = RunMode.Async)]
        [Alias("gains", "gain")]
        [Summary("The difference of a players stats in a period of time, only the gains..")]
        public async Task GetPositiveDelta([Remainder] PeriodAndMetricArguments arguments = null) {
            CommandPeriod = Period.Week;
            ExtractPeriodAndMetricArguments(arguments);
            _onlyPositiveDeltas = true;
            Response = await Delta();
        }

        [Name("Delta")]
        [Command("delta", RunMode = RunMode.Async)]
        [Alias("changes", "change")]
        [Summary("The difference of a players stats in a period of time, full change.")]
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
            ExtractPeriodAndMetricArguments(arguments);
            if (!CommandMetricType.HasValue) {
                CommandPeriod = Period.Week;
            }

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