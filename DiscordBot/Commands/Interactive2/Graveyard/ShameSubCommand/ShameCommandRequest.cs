using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.ShameSubCommand; 

public class ShameCommandRequest :ApplicationCommandRequestBase<ShameSubCommandDefinition>{
	public ShameCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}