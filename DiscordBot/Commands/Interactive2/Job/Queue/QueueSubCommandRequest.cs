using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Job.Queue;

public class QueueSubCommandRequest : ApplicationCommandRequestBase<QueueSubCommandDefinition> {
	public QueueSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.BotModerator;
}