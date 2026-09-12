using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Hosting.Footer;

public class FooterSubCommandRequest : ApplicationCommandRequestBase<FooterSubCommandDefinition> {
	public FooterSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotOwner;
}
