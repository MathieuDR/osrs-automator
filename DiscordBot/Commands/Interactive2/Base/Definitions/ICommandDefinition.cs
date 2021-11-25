namespace DiscordBot.Commands.Interactive2.Base.Definitions; 

public interface ICommandDefinition {
    public string Name { get; }
    public string Description { get; }
}

public interface IRootCommandDefinition : ICommandDefinition {
    public Guid Id { get; }
    Task<uint> GetCommandBuilderHash();
    Task<SlashCommandProperties> GetCommandProperties();
}

public interface ISubCommandDefinition<TParentCommand> : ISubCommandDefinition
    where TParentCommand : IRootCommandDefinition {
}

public interface ISubCommandDefinition : ICommandDefinition {
    Task<SlashCommandOptionBuilder> GetOptionBuilder();
}
