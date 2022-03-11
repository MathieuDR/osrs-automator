using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Drops.Request; 

public class RequestSubCommandRequest : ApplicationCommandRequestBase<RequestSubCommandDefinition> {
	public RequestSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanEventParticipant;
}
