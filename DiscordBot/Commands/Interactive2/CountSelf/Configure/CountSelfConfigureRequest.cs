using DiscordBot.Commands.Interactive2.CountSelf.Add;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.CountSelf.Configure; 

internal sealed class CountSelfConfigureRequest : ApplicationCommandRequestBase<CountSelfConfigureCommandDefinition> {
    public CountSelfConfigureRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
}
