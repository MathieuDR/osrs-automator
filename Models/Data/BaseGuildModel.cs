using Discord;

namespace DiscordBotFanatic.Models.Data {
    public class BaseGuildModel :BaseModel {
          public ulong GuildId { get; set; }
        public BaseGuildModel() { }

        public BaseGuildModel(IGuildUser user) : base(user) {
            GuildId = user.GuildId;
        }
    }
}