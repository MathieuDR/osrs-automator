using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using DiscordBotFanatic.Modules.Parameters;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services;

namespace DiscordBotFanatic.Modules {
    //[Group("stats")]
    //[Summary("Everything to do with wiseoldman.net")]
    public class WiseOldManModule : ModuleBase<SocketCommandContext> {
        private readonly WiseOldManConsumer _client;
        private readonly IDiscordBotRepository _repository;
        private readonly MessageConfiguration _messageConfiguration;

        private const string UsernameSummary = "The OSRS Player's username";
        private const string FilterArgumentsSummary = "Extra named, filter arguments";
        private const string MetricTypeSummary = "The skill metric";

        private string _messageUserDisplay = "";
        private string _osrsUsername = "";
        private MetricType? _metricType;
        private Period? _period;
        private Task<IUserMessage> _waitMessageTask;
        private int _timeToUpdate = 30;

        private BaseResponse _response;

        public WiseOldManModule(WiseOldManConsumer wiseOldManClient, IDiscordBotRepository repository,
            MessageConfiguration messageConfiguration) {
            _client = wiseOldManClient;
            _repository = repository;
            _messageConfiguration = messageConfiguration;
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

        protected override async void AfterExecute(CommandInfo command) {
            if (_response != null) {
                IUserMessage waitMessage = await _waitMessageTask;

                if (ValidateResponse(_response))
                {
                    var t = _response switch
                    {
                        PlayerResponse playerResponse => waitMessage.ModifyAsync(x =>
                            x.Embed = new Optional<Embed>(FormatEmbeddedFromPlayerResponse(playerResponse))),
                        DeltaResponse deltaResponse => waitMessage.ModifyAsync(x =>
                            x.Embed = new Optional<Embed>(FormatEmbeddedFromDeltaResponse(deltaResponse))),
                        RecordResponse recordResponse => waitMessage.ModifyAsync(x =>
                            x.Embed = new Optional<Embed>(FormatEmbeddedFromRecordResponse(recordResponse))),
                        _ => waitMessage.ModifyAsync(x =>
                        {
                            x.Embed = null;
                            x.Content = "Please contact admin. Response type unknown.";
                        })
                    };
                }

                // Error
#pragma warning disable 4014
                waitMessage.ModifyAsync(x =>
                {
#pragma warning restore 4014
                    x.Embed = null;
                    x.Content = _response.Message;
                });

            }

            base.AfterExecute(command);
        }


        [Command("get", RunMode = RunMode.Async)]
        [Summary("Current highscores of a player")]
        public async Task GetPlayer([Remainder]MetricOsrsArguments arguments = null) {
            ExtractMetricOsrsArguments(arguments);
            _response = await GetPlayerInfo();
        }

        [Command("delta", RunMode = RunMode.Async)]
        [Alias("gains", "gain")]
        [Summary("The gains of a player")]
        public async Task GetDelta([Remainder]PeriodAndMetricOsrsArguments arguments = null) {
            _period = Period.Week;
            ExtractPeriodAndMetricOsrsArguments(arguments);
            _response = await Delta();
        }
        
        [Command("record", RunMode = RunMode.Async)]
        [Alias("records")]
        [Summary("Get the record of specified time/metric")]
        public async Task GetRecords([Remainder]PeriodAndMetricOsrsArguments arguments = null) {
            _period = Period.Week;
            ExtractPeriodAndMetricOsrsArguments(arguments);
            _response = await GetPlayerRecord();
        }

        
        [Command("update", RunMode = RunMode.Async)]
        [Alias("new")]
        [Summary("Update your stats, this will also output your stats AFTER a refresh.")]
        public async Task UpdatePlayer([Summary(UsernameSummary)] string username = "") {
            _timeToUpdate = 1;
            if(!string.IsNullOrEmpty(username)) {
                _osrsUsername = username;
            }
            _response = await GetPlayerInfo();
        }


        [Command("set", RunMode = RunMode.Async)]
        [Alias("default")]
        [Summary("Set your standard player")]
        public async Task SetDefaultPlayer([Summary(UsernameSummary)] string username) {
            Embed embed = null;
            Player fromDb = _repository.GetPlayerByDiscordId(Context.User.Id.ToString());
            if (fromDb == null || fromDb.DefaultPlayerUsername.ToLowerInvariant() != username.ToLowerInvariant()) {
                var players = (await _client.SearchPlayerAsync(username)).ToList();
                PlayerResponse player;
                if (players == null || players.Count == 0) {
                    // Track player if we cant find him
                    player = await _client.TrackPlayerAsync(username);
                    if (ValidateResponse(player)) {
                        Player dbPlayer = new Player() {
                            DiscordId = Context.User.Id.ToString(), DefaultPlayerUsername = player.Username,
                            WiseOldManDefaultPlayerId = player.Id
                        };
                    }
                    else {
                        embed = GetCommonEmbedBuilder($"Error", player.Message).Build();
                    }
                }
                else if (players.Count == 1) {
                    var result = players.First();
                    Player dbPlayer = new Player() {
                        DiscordId = Context.User.Id.ToString(), DefaultPlayerUsername = result.Username,
                        WiseOldManDefaultPlayerId = result.Id
                    };
                    _repository.InsertOrUpdatePlayer(dbPlayer);
                }
                else {
                    embed = GetCommonEmbedBuilder($"Error",
                        $"Please use your full name. too many results ({players.Count})").Build();
                }
            }

            embed ??= GetCommonEmbedBuilder($"Done!", $"{username} is sucesfully set as your standard").Build();

#pragma warning disable 4014
            (await _waitMessageTask).ModifyAsync(x => {
#pragma warning restore 4014
                x.Embed = embed;
            });
        }

        

        #region API Wrapper

        private async Task<RecordResponse> GetPlayerRecord() {
            var playerResponse = await GetPlayerInfo();
            return await _client.GetPlayerRecordAsync(playerResponse.Id, _metricType, _period);
        }

        private async Task<PlayerResponse> GetPlayerInfo() {
            if (string.IsNullOrEmpty(_osrsUsername)) {
                #pragma warning disable 4014
                (await _waitMessageTask).ModifyAsync(x => {
                #pragma warning restore 4014
                    x.Embed = GetCommonEmbedBuilder($"Error!",
                            $"{_messageUserDisplay} You don't have a default set. Please use `set` to set a default character!")
                        .Build();
                    ;
                });

                return null;
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

        private bool ValidateResponse(BaseResponse response) {
            return response != null && string.IsNullOrEmpty(response.Message);
        }

        #endregion


        #region formatting

        private Embed PleaseWaitEmbed() {
            var builder = new EmbedBuilder() {
                Title = $"Please hang tight {_messageUserDisplay}, we're executing your command {new Emoji("\u2699")}",
                Description = $"{FanaticHelper.GetRandomDescription(_messageConfiguration)}{Environment.NewLine}This can actually take a while!" 
            };
            return builder.Build();
        }

        private EmbedBuilder GetCommonEmbedBuilder(string title, string description = null) {
            EmbedBuilder builder = new EmbedBuilder();

            builder.Title = title;
            builder.Description = description;
            builder.Timestamp = DateTimeOffset.UtcNow;
            builder.Footer = new EmbedFooterBuilder()
                {Text = $"Requested by {_messageUserDisplay}", IconUrl = Context.User.GetAvatarUrl()};

            return builder;
        }

        private Embed FormatEmbeddedFromPlayerResponse(PlayerResponse player) {
            var embed = GetCommonEmbedBuilder(
                $"{player.Username} stats from {player.UpdatedAt:dddd, dd/MM} at {player.UpdatedAt:HH:mm:ss}");
            FormatMetricsInEmbed(player.LatestSnapshot.MetricDictionary, embed);

            return embed.Build();
        }

        private Embed FormatEmbeddedFromDeltaResponse(DeltaResponse delta) {
            var embed = GetCommonEmbedBuilder($"{_osrsUsername} Gains!",
                $"Over a period of {delta.Interval}! Stoinks!");
            FormatMetricsInEmbed(delta.Metrics.DeltaMetricDictionary, embed);

            return embed.Build();
        }

        private Embed FormatEmbeddedFromRecordResponse(RecordResponse recordResponse) {
            string periodString = "period";
            if (_period.HasValue) {
                periodString = _period.Value.ToString().ToLowerInvariant();
            }

            var embed = GetCommonEmbedBuilder($"{_osrsUsername} Records!",$"The maximum experience earned over a {periodString}, What a beast!");

            var orderedList = recordResponse.Records.OrderBy(x => x.Period).ThenByDescending(x=> x.Value);
            bool isInline = orderedList.Count() > 4;

            foreach (Record record in orderedList) {
                string title = $"{record.Period} record for {record.Metric.ToString().ToLowerInvariant()}";
                if (isInline) {
                    title = $"{record.Metric.ToEmoji()} {record.Metric.ToString().ToLowerInvariant()}";
                }

                if (record.Value > 0) {
                    embed.AddField(title, record.ToString(), isInline);
                }
            }

            return embed.Build();
        }

        private void FormatMetricsInEmbed<T>(Dictionary<string, T> metricDictionary, EmbedBuilder embed) {
            foreach (KeyValuePair<string, T> kvp in metricDictionary) {
                MetricType type = Enum.Parse<MetricType>(kvp.Key, true);
                if (_metricType.HasValue && type != _metricType.Value) {
                    continue;
                }

                //embed.Description += $"{type.ToEmoji()} {kvp.Key}: {kvp.Value}{Environment.NewLine}{Environment.NewLine}";
                //embed.Description += $"{type.ToEmoji()} {kvp.Key}{Environment.NewLine}";
                //embed.Description += $"{kvp.Value}{Environment.NewLine}";
                //embed.AddField($"{type.ToEmoji()} {kvp.Key}", kvp.Value);
                embed.AddField($"{type.ToEmoji()} {kvp.Key}", kvp.Value, true);
            }
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

            _metricType = arguments.MetricType;
            _period = arguments.Period;
        }

        private void ExtractMetricOsrsArguments(MetricOsrsArguments arguments) {
            if (arguments == null) {
                return;
            }

            ExtractBaseOsrsArguments(arguments);

            _metricType = arguments.MetricType;
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