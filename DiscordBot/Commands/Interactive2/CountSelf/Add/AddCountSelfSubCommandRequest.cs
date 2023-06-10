using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.CountSelf.Add;

internal sealed  class AddCountSelfSubCommandRequest : ApplicationCommandRequestBase<AddCountSelfSubCommandDefinition> {
    public AddCountSelfSubCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}