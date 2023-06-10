using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Confirm;

internal sealed class ConfirmButtonRequest : MessageComponentRequestBase<ConfirmRootCommandDefinition> {
    public ConfirmButtonRequest(MessageComponentContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanModerator;
}