using WiseOldManConnector.Models.Output;

namespace DiscordBot.Models.Data {
    public class AutomatedJobState : BaseGuildModel {
        public Achievement LastPrintedAchievement { get; set; }
    }
}