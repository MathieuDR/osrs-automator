using System.Text;
using System.Text.Json;
using DiscordBot.Commands.Interactive2.Interfaces;
using HashDepot;

namespace DiscordBot.Commands.Interactive2.Ping;

public abstract class BaseSubCommand<TRoot> : ISubCommandDefinition<TRoot> where TRoot : IRootCommandDefinition {
    public Task<SlashCommandOptionBuilder> GetOptionBuilder() {
        throw new NotImplementedException();
    }

    public abstract string Name { get; }
    public abstract string Description { get; }
}

public abstract class BaseRootCommand : IRootCommandDefinition {
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

    private async Task<IEnumerable<ISubCommandDefinition<BaseRootCommand>>> GetSubcommands() {
        return await Task.FromResult(Array.Empty<ISubCommandDefinition<BaseRootCommand>>());
    }

    /// <summary>
    ///     Extend the builder. The Name and description is already set
    /// </summary>
    /// <param name="builder">Builder with name and description set</param>
    /// <returns>Fully build slash command builder</returns>
    protected abstract Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder);
}

public class PingRootCommand : BaseRootCommand, IRootCommandDefinition {
    public override string Name => "ping2";
    public override string Description => "Ping command through mediatr";

    public override Guid Id => Guid.Parse("341A00F5-AB4A-451F-8FA4-639D54EE658C");

    protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) {
        throw new NotImplementedException();
    }
}

public class PingSubCommand : ISubCommandDefinition<PingRootCommand> {
    public Task<SlashCommandOptionBuilder> GetOptionBuilder() {
        throw new NotImplementedException();
    }

    public string Name { get; }
    public string Description { get; }
}
