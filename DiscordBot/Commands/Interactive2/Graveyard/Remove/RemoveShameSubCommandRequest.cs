global using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Remove; 

public class RemoveShameSubCommandRequest : ApplicationCommandRequestBase<RemoveShameSubCommandDefinition> {
	public RemoveShameSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanModerator;
}
