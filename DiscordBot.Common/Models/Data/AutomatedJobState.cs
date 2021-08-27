using WiseOldManConnector.Models.Output;

namespace DiscordBot.Common.Models.Data {
    public class AutomatedJobState : BaseGuildModel {
        public Achievement LastPrintedAchievement { get; set; }
    }
}
