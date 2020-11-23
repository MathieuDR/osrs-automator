using System.Collections.Generic;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public abstract class Leaderboard: IBaseConnectorOutput{
        
        /// <summary>
        /// Offset
        /// </summary>
        public int Page { get; set; } = 0;

        /// <summary>
        /// Limit
        /// </summary>
        public int PageSize { get; set; } = 20;

    }

    public abstract class Leaderboard<T> : Leaderboard{
        protected Leaderboard() { }

        protected Leaderboard(List<T> items) {
            Members = items;
            PageSize = items.Count;
        }

        /// <summary>
        /// Response
        /// </summary>
        public List<T> Members { get; set; }

    }

    public abstract class MetricTypeLeaderboard<T> : Leaderboard<T> {
        protected MetricTypeLeaderboard() { }

        protected MetricTypeLeaderboard(MetricType metricType) {
            MetricType = metricType;
        }

        protected MetricTypeLeaderboard(List<T> items, MetricType metricType) : base(items) {
            MetricType = metricType;
        }

        /// <summary>
        ///  From Request
        /// </summary>
        public MetricType MetricType { get; set; }
    }

    // Should we use interfaces? probably..
    public abstract class MetricTypeAndPeriodLeaderboard<T> : MetricTypeLeaderboard<T> {
        protected MetricTypeAndPeriodLeaderboard() { }


        protected MetricTypeAndPeriodLeaderboard(MetricType metricType, Period period) : base(metricType) {
            Period = period;
        }

        protected MetricTypeAndPeriodLeaderboard(List<T> items, MetricType metricType, Period period) : base(items, metricType) {
            Period = period;
        }

        /// <summary>
        /// From Request
        /// </summary>
        public Period Period { get; set; }
    }

    public class RecordLeaderboard : MetricTypeAndPeriodLeaderboard<Record> {
        public RecordLeaderboard() { }
        public RecordLeaderboard(MetricType metricType, Period period) : base(metricType, period) { }
        public RecordLeaderboard(List<Record> items, MetricType metricType, Period period) : base(items, metricType, period) { }
    }

    public class HighscoreLeaderboard : MetricTypeLeaderboard<HighscoreMember> {
        public HighscoreLeaderboard() { }
        public HighscoreLeaderboard(MetricType metricType) : base(metricType) { }
        public HighscoreLeaderboard(List<HighscoreMember> items, MetricType metricType) : base(items, metricType) { }
    }

    public class DeltaLeaderboard : MetricTypeAndPeriodLeaderboard<DeltaMember> {
        public DeltaLeaderboard() { }
        public DeltaLeaderboard(MetricType metricType, Period period) : base(metricType, period) { }

        public DeltaLeaderboard(List<DeltaMember> items, MetricType metricType, Period period) :
            base(items, metricType, period) { }
    }

    public class CompetitionLeaderboard : MetricTypeLeaderboard<CompetitionParticipant> {
        public CompetitionLeaderboard() { }
        public CompetitionLeaderboard(MetricType metricType) : base(metricType) { }
        public CompetitionLeaderboard(List<CompetitionParticipant> items, MetricType metricType) : base(items, metricType) { }
    }
}