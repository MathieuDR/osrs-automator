using System.Text;
using System.Text.Json;
using HashDepot;

namespace DiscordBot.Commands.Interactive2.Base.Definitions;

public abstract class SubCommandDefinitionBase<TRoot> : ISubCommandDefinition<TRoot> where TRoot : IRootCommandDefinition {
    public async Task<SlashCommandOptionBuilder> GetOptionBuilder() {
        var builder = new SlashCommandOptionBuilder()
            .WithName(Name)
            .WithDescription(Description)
            .WithType(ApplicationCommandOptionType.SubCommand);

        builder = await ExtendOptionCommandBuilder(builder);

        return builder;
    }

    public abstract string Name { get; }
    public abstract string Description { get; }
    
    /// <summary>
    ///     Extend the builder. The Name and description is already set
    /// </summary>
    /// <param name="builder">Builder with name and description set</param>
    /// <returns>Fully build slash command builder</returns>
    protected abstract Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder);
}

public abstract class RootCommandDefinitionBase : IRootCommandDefinition {
    public abstract Guid Id { get; }

    public async Task<uint> GetCommandBuilderHash() {
        var command = await GetCommandBuilder();
        var json = JsonSerializer.Serialize(command);
        var stream = Encoding.ASCII.GetBytes(json);
        return XXHash.Hash32(stream);
    }

    public async Task<SlashCommandProperties> GetCommandProperties() {
        var builder = await GetCommandBuilder();
        return builder.Build();
    }

    public abstract string Name { get; }
    public abstract string Description { get; }

    public async Task<SlashCommandBuilder> GetCommandBuilder() {
        var builder = new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description);

        builder = await ExtendBaseSlashCommandBuilder(builder);

        await AddSubCommands(builder);

        return builder;
    }

    private async Task AddSubCommands(SlashCommandBuilder builder) {
        var subs = await GetSubcommands();
        var opts = await Task.WhenAll(subs.Select(x => x.GetOptionBuilder()));
        builder.AddOptions(opts);
    }

    private async Task<IEnumerable<ISubCommandDefinition<RootCommandDefinitionBase>>> GetSubcommands() {
        return await Task.FromResult(Array.Empty<ISubCommandDefinition<RootCommandDefinitionBase>>());
    }

    /// <summary>
    ///     Extend the builder. The Name and description is already set
    /// </summary>
    /// <param name="builder">Builder with name and description set</param>
    /// <returns>Fully build slash command builder</returns>
    protected abstract Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder);
}