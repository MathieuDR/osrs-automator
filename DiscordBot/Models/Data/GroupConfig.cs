using Discord;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Models.Data {
    public class GroupConfig : BaseGuildModel{
        public GroupConfig() { }
        public GroupConfig(ulong guildId, ulong discordId) : base(guildId, discordId) { }
        public GroupConfig(IGuildUser user) : base(user) { }

        public int WomGroupId { get; set; }
        public string WomVerificationCode { get; set; }
        public Group WomGroup { get; set; }
        public bool AutoAddNewAccounts { get; set; } = false;
    }
}