namespace DiscordBot.Common.Models.Data {
    public class BaseGuildModel : BaseModel {
        public BaseGuildModel() { }

        public BaseGuildModel(ulong guildId, ulong discordId) : base(discordId) {
            GuildId = guildId;
        }
        
        public ulong GuildId { get; set; }
    }
}