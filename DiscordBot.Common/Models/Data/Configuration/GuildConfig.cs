using DiscordBot.Common.Models.Data.Base;
using DiscordBot.Common.Models.Data.Counting;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Common.Models.Data.Configuration;

public class GuildConfig : BaseGuildModel {
	public GuildConfig() { }

	public GuildConfig(DiscordGuildId guildId, DiscordUserId creatorId) : base(guildId, creatorId) { }

	public int WomGroupId { get; set; }

	public string WomVerificationCode { get; set; }
	public Group WomGroup { get; set; }
	public string Timezone { get; set; }
	public bool AutoAddNewAccounts { get; set; } = false;
	public AutomatedMessagesConfig AutomatedMessagesConfig { get; set; } = new();
	public CountConfig CountConfig { get; set; }
	public CommandRoleConfig CommandRoleConfig { get; set; } = new();
}
