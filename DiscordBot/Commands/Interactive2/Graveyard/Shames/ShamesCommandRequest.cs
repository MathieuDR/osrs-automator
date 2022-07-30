using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shames;

public class ShamesCommandRequest : ApplicationCommandRequestBase<ShamesSubCommandDefinition> {
	public ShamesCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}