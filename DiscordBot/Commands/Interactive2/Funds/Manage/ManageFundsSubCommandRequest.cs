using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Funds.Manage;

public class ManageFundsSubCommandRequest : ApplicationCommandRequestBase<ManageFundsSubCommandDefinition> {
	public ManageFundsSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
}