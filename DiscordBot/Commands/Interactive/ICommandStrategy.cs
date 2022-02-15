namespace DiscordBot.Commands.Interactive;

public interface ICommandStrategy {
    public Task<Result> HandleApplicationCommand(ApplicationCommandContext context);
    public Task<Result> HandleComponentCommand(MessageComponentContext context);
    public Task<SlashCommandProperties[]> GetCommandPropertiesCollection(bool allBuilders);
    public Task<SlashCommandProperties> GetCommandProperties(string applicationCommandName);
    public Task<uint> GetCommandHash(string applicationCommandName);
    public Task<Result> HandleInteractiveCommand(BaseInteractiveContext context);
    public Task<IApplicationCommandHandler> GetHandler(string applicationCommandName);

    /// <summary>
    ///     Get the names and descriptions of the commands
    /// </summary>
    /// <returns>Name, Description of commands</returns>
    public IEnumerable<(string Name, string Description)> GetCommandDescriptions();

}
