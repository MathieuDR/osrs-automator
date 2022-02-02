using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Job.Configure;

public class ConfigureJobSubCommandRequest : ApplicationCommandRequestBase<ConfigureJobSubCommandDefinition> {
	public ConfigureJobSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
}