using System;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class Player : IBaseConnectorOutput{
        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public int CombatLevel { get; set; }
        public PlayerType Type { get; set; }
        public PlayerBuild Build { get; set; }
        public bool Flagged { get; set; }
        public DateTimeOffset? LastImportedAt { get; set; }
        public DateTimeOffset RegisteredAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Snapshot LatestSnapshot { get; set; }
        public int OverallExperience { get; set; }
        public GroupRole? Role{ get; set; }
        public double EffectiveHoursPlayer { get; set; }
        public double EffectiveHoursBossed { get; set; }
        public double TimeToMax { get; set; }
        public double TimeTo200m { get; set; }

    }
}