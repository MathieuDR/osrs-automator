using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Common.Models.Data.Configuration;

public class CommandRoleConfig {
	public Dictionary<DiscordRoleId, AuthorizationRoles> RoleIds { get; set; } = new();
	public Dictionary<DiscordUserId, AuthorizationRoles> UserIds { get; set; } = new();
}
