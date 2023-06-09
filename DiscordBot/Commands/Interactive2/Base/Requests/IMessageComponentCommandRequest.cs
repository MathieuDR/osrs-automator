namespace DiscordBot.Commands.Interactive2.Base.Requests;

public interface
	IMessageComponentCommandRequest<out TCommandDefinition> : ICommandRequest<TCommandDefinition, MessageComponentContext>
	where TCommandDefinition : ICommandDefinition {

}