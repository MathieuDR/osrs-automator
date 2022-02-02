namespace DiscordBot.Commands.Interactive2.Base.Definitions;

public interface IRootCommandDefinition : ICommandDefinition {
	Task<uint> GetCommandBuilderHash();
	Task<SlashCommandProperties> GetCommandProperties();
}