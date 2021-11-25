using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Commands.Interactive2.Ping.Definition;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Ping.CommandRequest; 

public class PingCommandRequest : ApplicationCommandRequestBase<PingRootCommandDefinition> {
    public PingCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.None;
}
