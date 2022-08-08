using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Count.Add;

public class AddCountSubCommandRequest : ApplicationCommandRequestBase<AddCountSubCommandDefinition> {
    public AddCountSubCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanModerator;
}