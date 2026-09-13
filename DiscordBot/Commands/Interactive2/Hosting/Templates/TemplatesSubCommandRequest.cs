using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Hosting.Templates;

public class TemplatesSubCommandRequest : ApplicationCommandRequestBase<TemplatesSubCommandDefinition> {
	public TemplatesSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotOwner;
}
