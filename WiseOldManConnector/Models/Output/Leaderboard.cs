using System.Collections.Generic;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public abstract class Leaderboard<T>{
        /// <summary>
        /// Response
        /// </summary>
        public List<T> Members { get; set; }

        /// <summary>
        /// Offset
        /// </summary>
        public int Page { get; set; } = 0;

        /// <summary>
        /// Limit
        /// </summary>
        public int PageSize { get; set; } = 20;
    }

    public abstract class MetricTypeLeaderboard<T> : Leaderboard<T> {
        /// <summary>
        ///  From Request
        /// </summary>
        public MetricType MetricType { get; set; }
    }

    // Should we use interfaces? probably..
    public abstract class MetricTypeAndPeriodLeaderboard<T> : MetricTypeLeaderboard<T> {
      /// <summary>
        /// From Request
        /// </summary>
        public Period Period { get; set; }
    }

    public class RecordLeaderboard : MetricTypeAndPeriodLeaderboard<Record> {
        
    }

    public class HiscoreLeaderboard : MetricTypeLeaderboard<HighscoreMember> {
         
    }

    public class DeltaLeaderboard : MetricTypeAndPeriodLeaderboard<DeltaMember> {

    }
}