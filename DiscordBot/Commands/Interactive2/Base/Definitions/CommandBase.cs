using System.Text;
using System.Text.Json;
using HashDepot;
using Microsoft.Extensions.DependencyInjection;
using RestSharp.Validation;

namespace DiscordBot.Commands.Interactive2.Base.Definitions;

public abstract class CommandDefinitionBase : ICommandDefinition { 
    protected ILogger Logger { get; }
    protected IServiceProvider ServiceProvider { get; }

    public CommandDefinitionBase(IServiceProvider serviceProvider) {
        ServiceProvider = serviceProvider;
        var loggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        Logger = loggerFactory.CreateLogger(GetType());
    }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public IEnumerable<(string optionName, Type optionType)> Options { get; } = new List<(string optionName, Type optionType)>();
    /// <summary>
    /// Set options in here with correct type.
    /// This will be used in the handlers to automatically get all options
    /// </summary>
    /// <returns></returns>
    protected virtual Task FillOptions() {
        return Task.CompletedTask;
    }
}

public abstract class SubCommandDefinitionBase<TRoot> : CommandDefinitionBase, ISubCommandDefinition<TRoot> where TRoot : IRootCommandDefinition {
    public async Task<SlashCommandOptionBuilder> GetOptionBuilder() {
        var builder = new SlashCommandOptionBuilder()
            .WithName(Name)
            .WithDescription(Description)
            .WithType(ApplicationCommandOptionType.SubCommand);

        builder = await ExtendOptionCommandBuilder(builder);

        return builder;
    }

    /// <summary>
    ///     Extend the builder. The Name and description is already set
    /// </summary>
    /// <param name="builder">Builder with name and description set</param>
    /// <returns>Fully build slash command builder</returns>
    protected abstract Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder);

    protected SubCommandDefinitionBase(IServiceProvider serviceProvider) : base(serviceProvider) { }
}

public abstract class RootCommandDefinitionBase: CommandDefinitionBase, IRootCommandDefinition {
    private readonly IEnumerable<ISubCommandDefinition> _subCommandDefinitions;

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
  
    public async Task<SlashCommandBuilder> GetCommandBuilder() {
        var builder = new SlashCommandBuilder()
            .WithName(Name)
            .WithDescription(Description);

        builder = await ExtendBaseSlashCommandBuilder(builder);

        await AddSubCommands(builder);
        
        ValidateCommandBuilder(builder);

        return builder;
    }

    private void ValidateCommandBuilder(SlashCommandBuilder builder) {
        if (builder.Name != builder.Name.ToLowerInvariant()) {
            throw new ArgumentException("Command name must be lowercase");
        }

        foreach (var option in builder.Options) {
            ValidateOption(option);
        }
    }

    private void ValidateOption(SlashCommandOptionBuilder option) {
        if(option.Name != option.Name.ToLowerInvariant()) {
            throw new ArgumentException($"Option name {option.Name} must be lowercase");
        }

        if (option.Type == ApplicationCommandOptionType.SubCommand) {
            foreach (var subOptions in option.Options) {
                ValidateOption(subOptions);
            }
        }
    }

    private async Task AddSubCommands(SlashCommandBuilder builder) {
        var subs = await GetSubcommands();
        var opts = await Task.WhenAll(subs.Select(x => x.GetOptionBuilder()));
        if(opts.Any()) {
            builder.AddOptions(opts);
        }
    }

    private async Task<IEnumerable<ISubCommandDefinition>> GetSubcommands() {
        return await Task.FromResult(_subCommandDefinitions);
    }

    /// <summary>
    ///     Extend the builder. The Name and description is already set
    /// </summary>
    /// <param name="builder">Builder with name and description set</param>
    /// <returns>Fully build slash command builder</returns>
    protected abstract Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder);

    protected RootCommandDefinitionBase(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider) {
        _subCommandDefinitions = subCommandDefinitions;
    }
}