using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Services;

namespace DiscordBotFanatic.Modules {
    [Name("Group")]
    [Group("Group")]
    public class GroupStatsModule : StatsBaseModule {
        
        private readonly WiseOldManConfiguration _wiseOldManConfiguration;
        private string _groupName = "";

        public GroupStatsModule(HighscoreService osrsHighscoreService, MessageConfiguration messageConfiguration, WiseOldManConfiguration wiseOldManConfiguration) : base(osrsHighscoreService, messageConfiguration) {
            _wiseOldManConfiguration = wiseOldManConfiguration;
            WiseOldManId = wiseOldManConfiguration.GroupId;
        }

        protected override string GetUrl() {
            return WiseOldManId > 0 ? $"https://wiseoldman.net/groups/{WiseOldManId}" : base.GetUrl();
        }

        protected override Embed GetEmbedResponse() {
            if (Response == null) {
                throw new NullReferenceException($"No response.");
            }

            switch (Response) {
                case GroupUpdateResponse groupUpdateResponse:
                    return FormatEmbeddedFromGroupUpdateResponse(groupUpdateResponse);
                case Dictionary<GroupMember, DeltaMetric> recordsPerGroupMember:
                    return FormatEmbeddedFromRecordsPerGroupMember(recordsPerGroupMember);
                default:
                    throw new Exception($"Response type unknown");
            }
        }



        public string Area {
            get { return string.IsNullOrEmpty(_groupName) ? "Group" : _groupName; }
        }

        #region commands

        [Command("update", RunMode = RunMode.Async)]
        [Summary("Update all players of a group.")]
        public async Task UpdateGroup([Summary("Optional Id, if not filled in, use the one from settings.")]int? id = null) {
            WiseOldManId = id ?? WiseOldManId;
            Response = await Service.UpdateGroupAsync(WiseOldManId);
        }

        [Command("top", RunMode = RunMode.Async)]
        [Summary("Get the top players in a metric. This will not update the group.")]
        public async Task GetTopPlayers([Remainder]PeriodAndMetricArguments arguments) {
            ExtractPeriodAndMetricArguments(arguments);
            if (!CommandMetricType.HasValue) {
                throw new ArgumentException($"Please fill in the metric to measure.");
            }

            Response = await Service.GetPlayerRecordsForGroupAsync(CommandMetricType.Value, CommandPeriod.HasValue ? CommandPeriod.Value : Period.Week, WiseOldManId);
        }

        #endregion

        #region api wrapper

        #endregion

        protected override void ExtractBaseArguments(BaseArguments baseArguments) {
            base.ExtractBaseArguments(baseArguments);

            if (!string.IsNullOrEmpty(Name)) {
                WiseOldManId = Service.GetGroupIdFromName(Name);
            }
        }

        #region formatting
        private Embed FormatEmbeddedFromGroupUpdateResponse(GroupUpdateResponse groupUpdateResponse) {
            var embed = GetCommonEmbedBuilder(Area, $"Update requested", groupUpdateResponse.Message);
            return embed.Build();
        }

        private Embed FormatEmbeddedFromRecordsPerGroupMember(Dictionary<GroupMember, DeltaMetric> recordsPerGroupMember) {
            throw new NotImplementedException($"Not implemented. WiseOldMan Implementing someting for us!");
            var embed = GetCommonEmbedBuilder(Area, $"Top members");
            var maxCoutner = Math.Min(10, recordsPerGroupMember.Count);
            recordsPerGroupMember = recordsPerGroupMember.OrderByDescending(x => x.Value.Experience?.Gained ?? x.Value.Score.Gained).ToDictionary(k => k.Key, k => k.Value);
            for (int i = 0; i <= maxCoutner; i++) {
                var kvp = recordsPerGroupMember.ElementAt(i);
                embed.AddField($"{kvp.Key.Username}", kvp.Value);
            }

            return embed.Build();
        }
        #endregion
    }
}