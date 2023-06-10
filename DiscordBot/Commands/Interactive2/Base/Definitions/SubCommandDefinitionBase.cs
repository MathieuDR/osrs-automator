namespace DiscordBot.Commands.Interactive2.Base.Definitions;

public  abstract class SubCommandDefinitionBase<TRoot> : CommandDefinitionBase, ISubCommandDefinition<TRoot> where TRoot : IRootCommandDefinition {
	protected SubCommandDefinitionBase(IServiceProvider serviceProvider) : base(serviceProvider) { }

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
}