using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Count.Log;

public class LogCountSubCommandRequest : ApplicationCommandRequestBase<LogCountSubCommandDefinition> {
    public LogCountSubCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}