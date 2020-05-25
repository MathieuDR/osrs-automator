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
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Services;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    [Name("Group")]
    //[Group("Group")]
    public class GroupStatsModule : StatsBaseModule {
        private readonly IGuildService _guildService;
        private bool _altDisplay;
        private string _groupName = "";
        private IGuildUser _user;


        public GroupStatsModule(HighscoreService osrsHighscoreService, MessageConfiguration messageConfiguration, WiseOldManConfiguration wiseOldManConfiguration, IGuildService guildService) : base(osrsHighscoreService, messageConfiguration) {
            _guildService = guildService;
            WiseOldManId = wiseOldManConfiguration.GroupId;
            _groupName = wiseOldManConfiguration.GroupName;
        }

        protected override void BeforeExecute(CommandInfo command) {
            if (Context == null) {
                return;
            }

            _user = Context.User as IGuildUser;
            base.BeforeExecute(command);
        }


        public string Area {
            get { return string.IsNullOrEmpty(_groupName) ? "Group" : _groupName; }
        }

        protected override string GetUrl() {
            return WiseOldManId > 0 ? $"https://wiseoldman.net/groups/{WiseOldManId}" : base.GetUrl();
        }

        protected override Embed GetEmbedResponse() {
            if (Response == null) {
                return null;
            }

            switch (Response) {
                case GroupUpdateResponse groupUpdateResponse:
                    return FormatEmbeddedFromGroupUpdateResponse(groupUpdateResponse);
                case List<LeaderboardMemberInfo> leaderboardInfos:
                    return FormatEmbeddedFromLeaderboardResponse(leaderboardInfos);
                case CreateGroupCompetitionResult createGroupCompetitionResult:
                    return FormatEmbeddedFromCreateGroupCompetitionResult(createGroupCompetitionResult);
                default:
                    throw new Exception($"Response type unknown");
            }
        }

        


        protected override void ExtractBaseArguments(BaseArguments baseArguments) {
            base.ExtractBaseArguments(baseArguments);

            if (!string.IsNullOrEmpty(Name)) {
                int newId = Service.GetGroupIdFromName(Name);
                if(newId > 0) {
                    WiseOldManId = Service.GetGroupIdFromName(Name);
                }
            }
        }

        #region commands

        [Command("group update", RunMode = RunMode.Async)]
        [Summary("Update all players of a group.")]
        public async Task UpdateGroup([Summary("Optional Id, if not filled in, use the one from settings.")]
            int? id = null) {
            WiseOldManId = id ?? WiseOldManId;
            Response = await Service.UpdateGroupAsync(WiseOldManId);
        }

        [Name("Leaderboards")]
        [Command("top", RunMode = RunMode.Async)]
        [Summary("Get the top players in a metric. This will not update the group.")]
        [Alias("leaderboards", "leaderboard", "ranking")]
        public async Task GetTopPlayers([Remainder] PeriodAndMetricArguments arguments) {
            ExtractPeriodAndMetricArguments(arguments);
            if (!CommandMetricType.HasValue) {
                throw new ArgumentException($"Please fill in the metric to measure.");
            }

            CommandPeriod ??= Period.Week;
            Response = await Service.GetPlayerRecordsForGroupAsync(CommandMetricType.Value, CommandPeriod.Value, WiseOldManId);
        }

        [Name("Leaderboards - Mobile")]
        [Command("alt-top", RunMode = RunMode.Async)]
        [Summary("Get the top players in a metric. This will not update the group.")]
        [Alias("alt-leaderboards", "alt-leaderboard", "alt-ranking", "alttop", "altleaderboards", "altleaderboard", "altranking")]
        public Task GetTopPlayersAlt([Remainder] PeriodAndMetricArguments arguments) {
            _altDisplay = true;
            return GetTopPlayers(arguments);
        }

        [Name("Create competition")]
        [Command("comp create")]
        [Summary("Creates a competition")]
        public async Task CreateCompetition(string title, MetricType metric, DateTime startDate, DateTime endDate) {
            if (!_guildService.DoesUserHavePermission(_user, Permissions.CompetitionManager)) {
                throw new UnauthorizedAccessException($"You don't have access towards event management");
            }

            Response = await _guildService.CreateGroupCompetition(title, metric, startDate, endDate);
        }

        [Name("Set guild competition")]
        [Command("comp set")]
        [Summary("Sets a competition for the server, where players can then query their rank and the leaderboards.")]
        public async Task SetGuildCompetition(int id) {
            if (!_guildService.DoesUserHavePermission(_user, Permissions.CompetitionManager)) {
                throw new UnauthorizedAccessException($"You don't have access towards competition management");
            }

            throw new NotImplementedException(nameof(SetGuildCompetition));
            // Send toe guild server to set it active
        }

        [Name("Clear guild competition")]
        [Command("comp clear")]
        [Summary("Ends or clears the set competition from the guild")]
        public async Task ClearGuildCompetition(int id) {
            if (!_guildService.DoesUserHavePermission(_user, Permissions.CompetitionManager)) {
                throw new UnauthorizedAccessException($"You don't have access towards competition management");
            }
            throw new NotImplementedException(nameof(ClearGuildCompetition));
        }

        // Time left command
        // Leaderboards for competition (Like leaderboards for deltas)

        #endregion

        #region api wrapper

        #endregion

        #region formatting

        private Embed FormatEmbeddedFromGroupUpdateResponse(GroupUpdateResponse groupUpdateResponse) {
            var embed = GetCommonEmbedBuilder(Area, $"Update requested", groupUpdateResponse.Message);
            return embed.Build();
        }

        private Embed FormatEmbeddedFromCreateGroupCompetitionResult(CreateGroupCompetitionResult createGroupCompetitionResult) {
            var embed = GetCommonEmbedBuilder(Area, $"Competition created", $"Please see direct message for the verification code.");
            embed.AddField("Title", createGroupCompetitionResult.Title);
            embed.AddField("Metric", createGroupCompetitionResult.Metric, true);
            embed.AddField("Starts", createGroupCompetitionResult.StartsAt, true);
            embed.AddField("Ends", createGroupCompetitionResult.EndsAt, true);
            embed.AddField("Participants", createGroupCompetitionResult.Participants.Count, true);

            Context.User.SendMessageAsync($"Competition {createGroupCompetitionResult.Title} administrator information.{Environment.NewLine}" + 
                                          $"Verification code: `{createGroupCompetitionResult.VerificationCode}`{Environment.NewLine}" + 
                                          $"Competition Id: `{createGroupCompetitionResult.Id}`");

            return embed.Build();
        }

        private Embed FormatEmbeddedFromLeaderboardResponse(List<LeaderboardMemberInfo> leaderboardInfos) {
            if (!leaderboardInfos.Any()) {
                return GetCommonEmbedBuilder($"No leaderboards for {Area}", $"For the period of {CommandPeriod.Value} and the metric {CommandMetricType.Value}.").Build();
            }

            int lastRank = leaderboardInfos.Count;
            int maxDisplay = 20;
            int startIndex = 0;
            int earchedItemRank = 0;
            LeaderboardMemberInfo searchedItem = null;
            if (!string.IsNullOrEmpty(Name)) {
                searchedItem = leaderboardInfos.SingleOrDefault(x => x.Info.Username.ToLowerInvariant() == Name.ToLowerInvariant());
                if (searchedItem == null) {
                    return GetCommonEmbedBuilder($"No rank for {Area}", $"For the period of {CommandPeriod.Value} and the metric {CommandMetricType.Value} for the user {Name}.").Build();
                }

                earchedItemRank = leaderboardInfos.IndexOf(searchedItem);
                startIndex = Math.Max(0, earchedItemRank - 10);
                int endIndex = Math.Min(leaderboardInfos.Count - startIndex, maxDisplay);
                leaderboardInfos = leaderboardInfos.GetRange(startIndex, endIndex);
            }

            var embed = GetCommonEmbedBuilder(Area, $"Leaderboards for {Area}");

            StringBuilder numberInline = new StringBuilder();
            StringBuilder nameInline = new StringBuilder();
            StringBuilder experienceInline = new StringBuilder();
            StringBuilder description = new StringBuilder();

            if (_altDisplay) {
                description.Append("#".PadLeft(3).PadRight(5));
                description.Append("Name".PadRight(14));
                description.Append("Experience");
                description.Append(Environment.NewLine);
            }

            int loops = Math.Min(maxDisplay, leaderboardInfos.Count);

            for (int i = 0; i < loops; i++) {
                var info = leaderboardInfos.ElementAt(i);
                string name = searchedItem != null && (info.Info.PlayerId == searchedItem.Info.PlayerId) ? info.Info.Username.ToUpper() : info.Info.Username;

                if (_altDisplay) {
                    description.Append($"{i + 1 + startIndex}, ".PadLeft(5));
                    description.Append(name.PadRight(14));
                    description.Append(info.Info.Gained.FormatNumber().PadLeft(6) + Environment.NewLine);
                } else {
                    numberInline.Append($"{i + 1 + startIndex}{Environment.NewLine}");
                    nameInline.Append(name + Environment.NewLine);
                    experienceInline.Append(info.Info.Gained.FormatNumber() + Environment.NewLine);
                }
            }

            if (searchedItem != null) {
                embed.Description = $"The player {Name} is rank {earchedItemRank+1}/{lastRank}.";
            }

            if (_altDisplay) {
                embed.Description += $"```{description}```";
            } else {
                embed.AddField("#", numberInline.ToString(), true);
                embed.AddField("Name", nameInline.ToString(), true);
                embed.AddField("Experience", experienceInline.ToString(), true);
                embed.AddEmptyField();
            }


            embed.AddField("Total ranks", (lastRank).ToString(), true);
            Debug.Assert(CommandMetricType != null, nameof(CommandMetricType) + " != null");
            embed.AddField("Metric", CommandMetricType.Value.ToString(), true);
            Debug.Assert(CommandPeriod != null, nameof(CommandPeriod) + " != null");
            embed.AddField("Period", CommandPeriod.Value.ToString(), true);
           

            if (!_altDisplay) {
                embed.AddField("Unreadable?", "For mobile users please use the command `group alt-top`");
            }
            

            return embed.Build();
        }

        #endregion
    }
}