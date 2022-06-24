namespace DiscordBot.Commands.Interactive2.Base.Requests;

public abstract class
	MessageComponentContextBase<TCommandDefinition> : CommandRequestBase<TCommandDefinition, MessageComponentContext>,
		IMessageComponentCommandRequest<TCommandDefinition> where TCommandDefinition : ICommandDefinition {
	protected MessageComponentContextBase(MessageComponentContext context) : base(context) { }
}