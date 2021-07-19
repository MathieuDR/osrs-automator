using Discord;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Models.Data {
    public class GuildConfig : BaseGuildModel {
        public GuildConfig() { }

        public GuildConfig(IGuildUser user) : this(user.GuildId, user.Id) { }

        public GuildConfig(ulong guildId, ulong discordId) : base(guildId, discordId) { }

        public int WomGroupId { get; set; }

        public string WomVerificationCode { get; set; }
        public Group WomGroup { get; set; }
        public bool AutoAddNewAccounts { get; set; } = false;
        public AutomatedMessagesConfig AutomatedMessagesConfig { get; set; } = new AutomatedMessagesConfig();
        public CountConfig CountConfig { get; set; }
    }
}