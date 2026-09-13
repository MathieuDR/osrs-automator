using DiscordBot.Commands.Interactive2.Hosting.Footer;
using DiscordBot.Commands.Interactive2.Hosting.Footers;
using DiscordBot.Commands.Interactive2.Hosting.Paid;
using DiscordBot.Commands.Interactive2.Hosting.Status;
using DiscordBot.Commands.Interactive2.Hosting.Templates;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Hosting;

public class HostingGuildAutoCompleteRequest : AutoCompleteCommandRequestBase<PaidSubCommandDefinition>,
	IAutoCompleteCommandRequest<StatusSubCommandDefinition>, IAutoCompleteCommandRequest<FooterSubCommandDefinition>,
	IAutoCompleteCommandRequest<FootersSubCommandDefinition>, IAutoCompleteCommandRequest<TemplatesSubCommandDefinition> {
	public HostingGuildAutoCompleteRequest(AutocompleteCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotOwner;
}
