namespace DiscordBot.Commands.Interactive2.Base.Requests;

public abstract class
	ApplicationCommandRequestBase<TCommandDefinition> : CommandRequestBase<TCommandDefinition, ApplicationCommandContext>,
		IApplicationCommandRequest<TCommandDefinition> where TCommandDefinition : ICommandDefinition{
	protected ApplicationCommandRequestBase(ApplicationCommandContext context) : base(context) { }
}