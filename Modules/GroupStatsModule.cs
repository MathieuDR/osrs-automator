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

namespace DiscordBotFanatic.Modules {
    [Name("Group")]
    [Group("Group")]
    public class GroupStatsModule : StatsBaseModule {
        private bool _altDisplay;
        private string _groupName = "";

        public GroupStatsModule(HighscoreService osrsHighscoreService, MessageConfiguration messageConfiguration, WiseOldManConfiguration wiseOldManConfiguration) : base(osrsHighscoreService, messageConfiguration) {
            WiseOldManId = wiseOldManConfiguration.GroupId;
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

                default:
                    throw new Exception($"Response type unknown");
            }
        }


        protected override void ExtractBaseArguments(BaseArguments baseArguments) {
            base.ExtractBaseArguments(baseArguments);

            if (!string.IsNullOrEmpty(Name)) {
                WiseOldManId = Service.GetGroupIdFromName(Name);
            }
        }

        #region commands

        [Command("update", RunMode = RunMode.Async)]
        [Summary("Update all players of a group.")]
        public async Task UpdateGroup([Summary("Optional Id, if not filled in, use the one from settings.")]
            int? id = null) {
            WiseOldManId = id ?? WiseOldManId;
            Response = await Service.UpdateGroupAsync(WiseOldManId);
        }

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

        [Command("alt-top", RunMode = RunMode.Async)]
        [Summary("Get the top players in a metric. This will not update the group.")]
        [Alias("alt-leaderboards", "alt-leaderboard", "alt-ranking", "alttop", "altleaderboards", "altleaderboard", "altranking")]
        public Task GetTopPlayersAlt([Remainder] PeriodAndMetricArguments arguments) {
            _altDisplay = true;
            return GetTopPlayers(arguments);
        }

        #endregion

        #region api wrapper

        #endregion

        #region formatting

        private Embed FormatEmbeddedFromGroupUpdateResponse(GroupUpdateResponse groupUpdateResponse) {
            var embed = GetCommonEmbedBuilder(Area, $"Update requested", groupUpdateResponse.Message);
            return embed.Build();
        }

        private Embed FormatEmbeddedFromLeaderboardResponse(List<LeaderboardMemberInfo> leaderboardInfos) {
            var embed = GetCommonEmbedBuilder(Area, $"Leaderboards for <groupname>.");

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

            int loops = Math.Min(20, leaderboardInfos.Count);

            for (int i = 0; i < loops; i++) {
                var info = leaderboardInfos.ElementAt(i);
                if (_altDisplay) {
                    description.Append($"{i + 1}, ".PadLeft(5));
                    description.Append(info.Info.Username.PadRight(14));
                    description.Append(info.Info.Gained.FormatNumber().PadLeft(6) + Environment.NewLine);
                } else {
                    numberInline.Append($"{i + 1}{Environment.NewLine}");
                    nameInline.Append(info.Info.Username + Environment.NewLine);
                    experienceInline.Append(info.Info.Gained.FormatNumber() + Environment.NewLine);
                }
            }

            if (_altDisplay) {
                embed.Description = $"```{description}```";
            } else {
                embed.AddField("#", numberInline.ToString(), true);
                embed.AddField("Name", nameInline.ToString(), true);
                embed.AddField("Experience", experienceInline.ToString(), true);
                embed.AddEmptyField();
            }

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