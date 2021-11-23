using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Interfaces;

public interface IBaseDefinition {
    public string Name { get; }
    public string Description { get; }
    public AuthorizationRoles MinimumAuthorizationRole { get; }
}

public interface ICommandDefinition : IBaseDefinition {
    public Guid Id { get; }
    Task<uint> GetCommandBuilderHash();
    Task<SlashCommandProperties> GetCommandProperties();
}

public interface ISubCommandDefinition: IBaseDefinition{
    public Type Parent { get; }
    Task<SlashCommandOptionBuilder> GetOptionBuilder();
}

public interface ICommandHandler<in T, TT, TU> 
    where T: BaseInteractiveContext<TT> 
    where TT : SocketInteraction
    where TU : IBaseDefinition
{
    public bool CanHandle(T context);
    public Task<Result> Handle(T context);
}