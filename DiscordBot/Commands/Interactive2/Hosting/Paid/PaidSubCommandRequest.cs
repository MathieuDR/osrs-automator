using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Hosting.Paid;

public class PaidSubCommandRequest : ApplicationCommandRequestBase<PaidSubCommandDefinition> {
	public PaidSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotOwner;
}
