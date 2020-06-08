using System;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public class CompetitionInfo : BaseInfo<Participant> {
        public string Name { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public Group Group { get; set; }
        public int Id { get; set; }

        public CompetitionInfo(int id, string competitionName, DateTime competitionStartTime, DateTime competitionEndTime, Group competitionGroup, Participant info, MetricType type) : base(info, type) {
            Name = competitionName;
            StartTime = competitionStartTime;
            EndTime = competitionEndTime;
            Group = competitionGroup;
            Id = id;
        }

        public CompetitionInfo(int id, string competitionName, DateTime competitionStartTime, DateTime competitionEndTime, Group competitionGroup, Participant info, string type)  : this(id, competitionName, competitionStartTime, competitionEndTime, competitionGroup, info, type.ToMetricType()){ }

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