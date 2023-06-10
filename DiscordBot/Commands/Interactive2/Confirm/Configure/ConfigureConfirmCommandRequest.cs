using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Confirm.Configure; 

internal sealed class ConfigureConfirmCommandRequest : ApplicationCommandRequestBase<ConfigureConfirmCommandDefinition> {
    public ConfigureConfirmCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
}
