using System;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class Player {
        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public int CombatLevel { get; set; }
        public PlayerType Type { get; set; }
        public DateTimeOffset? LastImportedAt { get; set; }
        public DateTimeOffset RegisteredAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public Snapshot LatestSnapshot { get; set; }

    }
}