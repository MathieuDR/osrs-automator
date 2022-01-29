using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.ShameSubCommand;

public class ShameAutoCompleteRequest :AutoCompleteCommandRequestBase<ShameSubCommandDefinition>{
	public ShameAutoCompleteRequest(AutocompleteCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole=> AuthorizationRoles.ClanMember;
}
