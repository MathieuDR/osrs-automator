using System.Text;
using System.Text.Json;
using HashDepot;

namespace DiscordBot.Commands.Interactive; 

public abstract class ApplicationCommandHandler : IApplicationCommandHandler {
    public ApplicationCommandHandler(string name, string description, ILogger logger) {
        Name = name.ToLowerInvariant().Trim().Replace(" ", "-");
        Description = description;
        Logger = logger;
    }

    public ILogger Logger { get; }
    public abstract Guid Id { get; }
    public abstract AuthorizationRoles MinimumAuthorizationRole { get; }
    public string Name { get; }
    public string Description { get; }
    public virtual bool GlobalRegister => false;

    public async Task<SlashCommandBuilder> GetCommandBuilder() {
        Logger.LogInformation("Creating SlashCommandBuilder for {command}: {description}", Name, Description);
        var builder = new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description);

        builder = await ExtendSlashCommandBuilder(builder);

        return builder;
    }

    public async Task<SlashCommandProperties> GetCommandProperties() {
        var builder = await GetCommandBuilder();
        return builder.Build();
    }

    protected string SubCommand(params string[] ids) {
        var stringsToJoin = new string[ids.Length + 1];
        stringsToJoin[0] = Name;
        Array.Copy(ids, 0, stringsToJoin, 1, ids.Length);

        return string.Join(".", stringsToJoin);
    }

    public abstract Task<Result> HandleCommandAsync(ApplicationCommandContext context);

    public abstract Task<Result> HandleComponentAsync(MessageComponentContext context);

    public virtual Task RemoveComponent(MessageComponentContext context) {
        return Task.CompletedTask;
    }

    public virtual bool CanHandle(ApplicationCommandContext context) {
        return string.Equals(context.InnerContext.Data.Name, Name, StringComparison.InvariantCultureIgnoreCase);
    }

    public bool CanHandle(MessageComponentContext context) {
        return string.Equals(context.CustomIdParts.FirstOrDefault() ?? "", Name, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    ///     Extend the builder. The Name and description is already set
    /// </summary>
    /// <param name="builder">Builder with name and description set</param>
    /// <returns>Fully build slash command builder</returns>
    protected abstract Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder);

    public async Task<uint> GetCommandBuilderHash() {
        var command = await GetCommandBuilder();
        var json = JsonSerializer.Serialize(command);
        var stream = Encoding.ASCII.GetBytes(json);
        return XXHash.Hash32(stream);
    }
}