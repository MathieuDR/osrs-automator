using System.Text;
using System.Text.Json;
using HashDepot;

namespace DiscordBot.Commands.Interactive2.Base.Definitions;

public abstract class RootCommandDefinitionBase : CommandDefinitionBase, IRootCommandDefinition {
	private readonly IEnumerable<ISubCommandDefinition> _subCommandDefinitions;

	protected RootCommandDefinitionBase(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) :
		base(serviceProvider) => _subCommandDefinitions = subCommandDefinitions;

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
		if (option.Name != option.Name.ToLowerInvariant()) {
			throw new ArgumentException($"Option name {option.Name} must be lowercase");
		}

		if (option.Type == ApplicationCommandOptionType.SubCommand) {
			if (option.Options is not null) {
				foreach (var subOptions in option.Options) {
					ValidateOption(subOptions);
				}
			}
		}
	}

	private async Task AddSubCommands(SlashCommandBuilder builder) {
		var subs = await GetSubcommands();
		var opts = await Task.WhenAll(subs.Select(x => x.GetOptionBuilder()));
		if (opts.Any()) {
			builder.AddOptions(opts);
		}
	}

	private async Task<IEnumerable<ISubCommandDefinition>> GetSubcommands() => await Task.FromResult(_subCommandDefinitions);

	/// <summary>
	///     Extend the builder. The Name and description is already set
	/// </summary>
	/// <param name="builder">Builder with name and description set</param>
	/// <returns>Fully build slash command builder</returns>
	protected virtual Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) => Task.FromResult(builder);
}