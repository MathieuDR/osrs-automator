using DiscordBot.Models.Enums;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Models {
    public class CreateCompetition {
        public ulong GuildId { get; set; }
        public MetricTypeCategory MetricTypeCategory { get; set; }
        public MetricType MetricType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public CompetitionType CompetitionType { get; set; }
    }
}
