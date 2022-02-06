using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Users;

public class UserSubCommandRequest : ApplicationCommandRequestBase<UserSubCommandDefinition> {
	public UserSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}