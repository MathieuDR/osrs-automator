using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shame; 

public class ShameCommandRequest :ApplicationCommandRequestBase<ShameSubCommandDefinition>{
	public ShameCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}

public class ShameAutoCompleteRequest :AutoCompleteCommandRequestBase<ShameSubCommandDefinition>{
	public ShameAutoCompleteRequest(AutocompleteCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole=> AuthorizationRoles.ClanMember;
}


