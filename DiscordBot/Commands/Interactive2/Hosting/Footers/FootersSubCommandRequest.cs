using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Hosting.Footers;

public class FootersSubCommandRequest : ApplicationCommandRequestBase<FootersSubCommandDefinition> {
	public FootersSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotOwner;
}
