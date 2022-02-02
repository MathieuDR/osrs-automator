namespace DiscordBot.Commands.Interactive2.Base.Definitions; 

public interface ICommandDefinition {
    public string Name { get; }
    public string Description { get; }
    
    public IEnumerable<(String optionName, Type optionType)> Options { get; } 
}

public interface ISubCommandDefinition : ICommandDefinition {
    Task<SlashCommandOptionBuilder> GetOptionBuilder();
}
