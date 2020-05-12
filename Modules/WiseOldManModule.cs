using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services;

namespace DiscordBotFanatic.Modules {
    //[Group("stats")]
    //[Summary("Everything to do with wiseoldman.net")]

    [Name("Player stats")]
    public class WiseOldManModule : ModuleBase<SocketCommandContext> {
        private readonly WiseOldManConsumer _client;
        private readonly IDiscordBotRepository _repository;
        private readonly MessageConfiguration _messageConfiguration;

        private const string UsernameSummary = "The OSRS Player's username";

        private string _messageUserDisplay = "";
        private string _osrsUsername = "";
        private MetricType? _metricType;
        private Period? _period;
        private IUserMessage _waitMessage;

        // Don't forget to update the GET function summary!
        private int _timeToUpdate = 30;

        private BaseResponse _response;
        private Task<IUserMessage> _waitMessageTask;

        public WiseOldManModule(WiseOldManConsumer wiseOldManClient, IDiscordBotRepository repository, MessageConfiguration messageConfiguration) {
            _client = wiseOldManClient;
            _repository = repository;
            _messageConfiguration = messageConfiguration;
        }

        public IUserMessage WaitMessage {
            get {
                try {
                    return _waitMessage ??= _waitMessageTask.Result;
                }
                catch {
                    return null;
                }
            }
        }

        protected override void BeforeExecute(CommandInfo command) {
            if (Context == null) {
                return;
            }

            _messageUserDisplay = Context.User.Username;

            SocketGuildUser user = Context.Guild?.Users.SingleOrDefault(x => x.Id == Context.User.Id);

            if (user != null) {
                _messageUserDisplay = user.Nickname;
            }

            _osrsUsername = GetUserNameFromUser();

            _waitMessageTask = ReplyAsync(embed: PleaseWaitEmbed());


            base.BeforeExecute(command);
        }

        protected override void AfterExecute(CommandInfo command) {
            if (_response != null) {
                Embed embedResponse;
                ValidateResponse(_response);
                switch (_response) {
                    case PlayerResponse playerResponse:
                        embedResponse = FormatEmbeddedFromPlayerResponse(playerResponse);
                        break;
                    case DeltaResponse deltaResponse:
                        embedResponse = FormatEmbeddedFromDeltaResponse(deltaResponse);
                        break;
                    case RecordResponse recordResponse:
                        embedResponse = FormatEmbeddedFromRecordResponse(recordResponse);
                        break;
                    default:
                        throw new Exception($"Response type unknown");
                }

                if (WaitMessage != null) {
                    WaitMessage.ModifyAsync(x => x.Embed = new Optional<Embed>(embedResponse));
                }
                else {
                    ReplyAsync(embed: embedResponse);
                }
            }

            base.AfterExecute(command);
        }


        #region player

        [Name("Get")]
        [Command("get", RunMode = RunMode.Async)]
        [Summary("Current highscores of a player, Will try to update if older then 30 minutes.")]
        public async Task GetPlayer([Remainder] MetricOsrsArguments arguments = null) {
            ExtractMetricOsrsArguments(arguments);
            _response = await GetPlayerInfo();
        }

        [Name("Delta")]
        [Command("delta", RunMode = RunMode.Async)]
        [Alias("gains", "gain")]
        [Summary("The difference of a players stats in a period of time.")]
        public async Task GetDelta([Remainder] PeriodAndMetricOsrsArguments arguments = null) {
            _period = Period.Week;
            ExtractPeriodAndMetricOsrsArguments(arguments);
            _response = await Delta();
        }

        [Name("Records")]
        [Command("records", RunMode = RunMode.Async)]
        [Alias("record")]
        [Summary("Record gains of a specific stat and/or period.")]
        public async Task GetRecords([Remainder] PeriodAndMetricOsrsArguments arguments = null) {
            _period = Period.Week;
            ExtractPeriodAndMetricOsrsArguments(arguments);
            _response = await GetPlayerRecord();
        }

        [Name("Update")]
        [Command("update", RunMode = RunMode.Async)]
        [Alias("new")]
        [Summary("Force refresh your stats if older then 1 minute, and display them like the `get` command")]
        public async Task UpdatePlayer([Summary(UsernameSummary)] string username = "") {
            _timeToUpdate = 1;
            if (!string.IsNullOrEmpty(username)) {
                _osrsUsername = username;
            }

            _response = await GetPlayerInfo();
        }

        [Name("Set default character")]
        [Command("set", RunMode = RunMode.Async)]
        [Alias("default")]
        [Summary("Set your standard player to use within the `player` module.")]
        public async Task SetDefaultPlayer([Summary(UsernameSummary)] string username) {
            Embed embed;
            Player fromDb = _repository.GetPlayerByDiscordId(Context.User.Id.ToString());
            if (fromDb == null || fromDb.DefaultPlayerUsername.ToLowerInvariant() != username.ToLowerInvariant()) {
                var players = (await _client.SearchPlayerAsync(username)).ToList();
                PlayerResponse player;
                Player dbPlayer;
                if (players.Count == 0) {
                    // Track player if we cant find him
                    player = await _client.TrackPlayerAsync(username);
                    ValidateResponse(player);
                    dbPlayer = new Player() {
                        DiscordId = Context.User.Id.ToString(), DefaultPlayerUsername = player.Username,
                        WiseOldManDefaultPlayerId = player.Id
                    };
                }
                else if (players.Count == 1) {
                    var result = players.First();
                    dbPlayer = new Player() {
                        DiscordId = Context.User.Id.ToString(), DefaultPlayerUsername = result.Username,
                        WiseOldManDefaultPlayerId = result.Id
                    };
                }
                else {
                    throw new ArgumentException($"Please use your full OSRS name. Too many results using {_osrsUsername} ({players.Count}).");
                }


                _repository.InsertOrUpdatePlayer(dbPlayer);
            }

            embed = GetCommonEmbedBuilder($"Done!", $"{username} is sucesfully set as your standard").Build();

            await WaitMessage.ModifyAsync(x => { x.Embed = embed; });
        }

        #endregion

        #region API Wrapper

        private async Task<RecordResponse> GetPlayerRecord() {
            var playerResponse = await GetPlayerInfo();
            return await _client.GetPlayerRecordAsync(playerResponse.Id, _metricType, _period);
        }

        private async Task<PlayerResponse> GetPlayerInfo() {
            if (string.IsNullOrEmpty(_osrsUsername)) {
                throw new ArgumentException($"{_messageUserDisplay} you don't have a default set. Use the `set` command.");
            }

            var response = await ShouldUpdate();

            if (response != null) {
                return response;
            }

            return await _client.TrackPlayerAsync(_osrsUsername);
        }

        private async Task<PlayerResponse> ShouldUpdate() {
            var response = await _client.GetPlayerAsync(_osrsUsername);
            if (response.UpdatedAt.AddMinutes(_timeToUpdate) >= DateTime.UtcNow) {
                return response;
            }

            return null;
        }

        private async Task<DeltaResponse> Delta() {
            // checks if we need to update or not, if so updates.
            var response = await GetPlayerInfo();

            Debug.Assert(_period != null, nameof(_period) + " != null");
            return await _client.DeltaPlayerAsync(response.Id, _period.Value);
        }

        private void ValidateResponse(BaseResponse response) {
            if (response == null) {
                throw new ArgumentException($"We did not receive a response. Pleas try again later or contact the administration.");
            }

            if (!string.IsNullOrEmpty(response.Message)) {
                throw new ArgumentException(response.Message);
            }
        }

        #endregion


        #region formatting

        private Embed PleaseWaitEmbed() {
            var builder = new EmbedBuilder() {
                Title = $"Please hang tight {_messageUserDisplay}, we're executing your command {new Emoji("\u2699")}",
                Description = $"{FanaticHelper.GetRandomDescription(_messageConfiguration)}{Environment.NewLine}This can actually take a while!"
            };
            builder.WithFooter(Context.Message.Id.ToString());
            return builder.Build();
        }

        private EmbedBuilder GetCommonEmbedBuilder(string title, string description = null) {
            EmbedBuilder builder = new EmbedBuilder();

            builder.Title = title;
            builder.Description = description;
            builder.Timestamp = DateTimeOffset.UtcNow;
            builder.Footer = new EmbedFooterBuilder() {Text = $"Requested by {_messageUserDisplay}", IconUrl = Context.User.GetAvatarUrl()};

            return builder;
        }

        private Embed FormatEmbeddedFromPlayerResponse(PlayerResponse player) {
            var embed = GetCommonEmbedBuilder($"{player.Username} stats from {player.UpdatedAt:dddd, dd/MM} at {player.UpdatedAt:HH:mm:ss}");
            FormatMetricsInEmbed(player.LatestSnapshot.MetricDictionary, embed);

            return embed.Build();
        }

        private Embed FormatEmbeddedFromDeltaResponse(DeltaResponse delta) {
            var embed = GetCommonEmbedBuilder($"{_osrsUsername} Gains!", $"Over a period of {delta.Interval}! Stoinks!");
            FormatMetricsInEmbed(delta.Metrics.AllDeltaMetrics, embed);

            return embed.Build();
        }

        private Embed FormatEmbeddedFromRecordResponse(RecordResponse recordResponse) {
            string periodString = "period";
            if (_period.HasValue) {
                periodString = _period.Value.ToString().ToLowerInvariant();
            }


            var embed = GetCommonEmbedBuilder($"{_osrsUsername} Records!", $"The maximum experience earned over a {periodString}, What a beast!");

            if (!recordResponse.Records.Any(x => x.Value > 0)) {
                embed.Description = $"No records for {_osrsUsername} over the period of a {periodString}";
                if (_metricType.HasValue) {
                    embed.Description += $" in the metric {_metricType.ToString()?.ToLowerInvariant()}";
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
                if ((_metricType.HasValue && type != _metricType.Value)|| !type.IsSkillMetric()) {
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
            builder.Append($"No records or stats for {_osrsUsername}");

            if (_period.HasValue) {
                builder.Append($" over a period of {_period.Value}");
            }
            if (_metricType.HasValue) {
                builder.Append($" in the metric {_metricType.ToString()?.ToLowerInvariant()}");
            }

            builder.Append(".");
            embed.Description = builder.ToString();
        }

        #endregion

        private string GetUserNameFromUser() {
            Player player = _repository.GetPlayerByDiscordId(Context.User.Id.ToString());

            if (player == null) {
                return null;
            }

            return player.DefaultPlayerUsername;
        }

        private void ExtractPeriodAndMetricOsrsArguments(PeriodAndMetricOsrsArguments arguments) {
            if (arguments == null) {
                return;
            }

            ExtractBaseOsrsArguments(arguments);

            _metricType = arguments.MetricType ?? _metricType;
            _period = arguments.Period ?? _period;
        }

        private void ExtractMetricOsrsArguments(MetricOsrsArguments arguments) {
            if (arguments == null) {
                return;
            }

            ExtractBaseOsrsArguments(arguments);

            _metricType = arguments.MetricType ?? _metricType;
        }

        private void ExtractBaseOsrsArguments(BaseOsrsArguments baseOsrsArguments) {
            if (baseOsrsArguments == null) {
                return;
            }

            if (!string.IsNullOrEmpty(baseOsrsArguments.Username)) {
                _osrsUsername = baseOsrsArguments.Username;
            }
        }
    }
}