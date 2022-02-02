namespace DiscordBot.Commands.Interactive2.Base.Definitions;

public interface ISubCommandDefinition<TParentCommand> : ISubCommandDefinition
	where TParentCommand : IRootCommandDefinition {
}