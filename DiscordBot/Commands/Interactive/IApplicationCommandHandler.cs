using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive;

public interface IApplicationCommandHandler {
    public string Name { get; }
    public string Description { get; }
    public bool GlobalRegister { get; }
    public AuthorizationRoles MinimumAuthorizationRole { get; }
    Task<SlashCommandProperties> GetCommandProperties();
    Task<Result> HandleCommandAsync(ApplicationCommandContext context);
    Task<Result> HandleAutocompleteAsync(AutocompleteCommandContext context);
    Task<Result> HandleComponentAsync(MessageComponentContext context);
    public bool CanHandle(ApplicationCommandContext context);
    public bool CanHandle(AutocompleteCommandContext context);
    public bool CanHandle(MessageComponentContext context);
    Task<uint> GetCommandBuilderHash();
}
