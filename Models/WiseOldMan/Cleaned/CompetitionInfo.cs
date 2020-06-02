using System;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public class CompetitionInfo : BaseInfo<Participant> {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Group Group { get; set; }

        public CompetitionInfo(string competitionName, DateTime competitionStartTime, DateTime competitionEndTime, Group competitionGroup, Participant info, MetricType type) : base(info, type) {
            Name = competitionName;
            StartTime = competitionStartTime;
            EndTime = competitionEndTime;
            Group = competitionGroup;
        }

        public CompetitionInfo(string competitionName, DateTime competitionStartTime, DateTime competitionEndTime, Group competitionGroup, Participant info, string type)  : this(competitionName, competitionStartTime, competitionEndTime, competitionGroup, info, type.ToMetricType()){ }

        public bool HasGained {
            get {
                if (Info.Progress != null) {
                    return Info.Progress.Gained > 0;
                }

                return false;
            }
        }
    }
}