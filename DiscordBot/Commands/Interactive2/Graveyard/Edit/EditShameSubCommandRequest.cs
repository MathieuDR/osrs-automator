using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Edit; 

public class EditShameSubCommandRequest : ApplicationCommandRequestBase<EditShameSubcommandDefinition> {
	public EditShameSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanModerator;
}
