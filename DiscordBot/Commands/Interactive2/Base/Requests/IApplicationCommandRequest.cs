namespace DiscordBot.Commands.Interactive2.Base.Requests;

public interface
	IApplicationCommandRequest<out TCommandDefinition> : ICommandRequest<TCommandDefinition, ApplicationCommandContext>
	where TCommandDefinition : ICommandDefinition { }