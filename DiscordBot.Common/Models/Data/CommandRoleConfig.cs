using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Common.Models.Data;

public class CommandRoleConfig {
    public Dictionary<ulong, AuthorizationRoles> RoleIds { get; set; } = new();
    public Dictionary<ulong, AuthorizationRoles> UserIds { get; set; } = new();
}