using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Ping;

public class NormalPingCommandRequest : ApplicationCommandRequestBase<NormalSubCommandDefinition> {
	public NormalPingCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.None;
}