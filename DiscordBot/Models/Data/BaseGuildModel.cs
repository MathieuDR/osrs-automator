using Discord;

namespace DiscordBotFanatic.Models.Data {
    public class BaseGuildModel : BaseModel {
        public BaseGuildModel() { }

        public BaseGuildModel(ulong guildId, ulong discordId) : base(discordId) {
            GuildId = guildId;
        }

        public BaseGuildModel(IGuildUser user) : base(user) {
            GuildId = user.GuildId;
        }

        public ulong GuildId { get; set; }
    }
}