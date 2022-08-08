using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Count.Export;

public class ExportCountSubCommandRequest : ApplicationCommandRequestBase<ExportCountSubCommandDefinition> {
    public ExportCountSubCommandRequest(ApplicationCommandContext context) : base(context) { }
    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
}