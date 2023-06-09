namespace DiscordBot.Commands.Interactive2.Base.Requests;

public abstract class
	MessageComponentRequestBase<TCommandDefinition> : CommandRequestBase<TCommandDefinition, MessageComponentContext>,
		IMessageComponentCommandRequest<TCommandDefinition> where TCommandDefinition : ICommandDefinition {
	protected MessageComponentRequestBase(MessageComponentContext context) : base(context) { }
}