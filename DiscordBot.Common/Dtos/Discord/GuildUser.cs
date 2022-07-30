global using DiscordBot.Common.Identities;

namespace DiscordBot.Common.Dtos.Discord;

public class GuildUser : GuildEntity {
	public DiscordUserId Id { get; set; }
	public string Username { get; set; }
	public bool IsBot { get; set; }
	public List<DiscordRoleId> RoleIds { get; set; }
}
