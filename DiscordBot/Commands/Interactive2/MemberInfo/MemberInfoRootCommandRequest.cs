using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.MemberInfo;

public class MemberInfoRootCommandRequest : ApplicationCommandRequestBase<MemberInfoRootCommandDefinition> {
	public MemberInfoRootCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanOwner;
}