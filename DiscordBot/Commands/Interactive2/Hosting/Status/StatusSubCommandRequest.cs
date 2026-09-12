using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Hosting.Status;

public class StatusSubCommandRequest : ApplicationCommandRequestBase<StatusSubCommandDefinition> {
	public StatusSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotOwner;
}
