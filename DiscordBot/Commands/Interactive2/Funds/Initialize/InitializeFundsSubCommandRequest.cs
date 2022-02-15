using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Funds.Initialize;

public class InitializeFundsSubCommandRequest: ApplicationCommandRequestBase<InitializeFundsSubRequestDefinition> {
	public InitializeFundsSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
}