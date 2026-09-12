using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Hosting.Remind;

public class RemindSubCommandRequest : ApplicationCommandRequestBase<RemindSubCommandDefinition> {
	public RemindSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotOwner;
}
