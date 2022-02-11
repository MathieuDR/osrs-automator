using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Graveyard.Users; 

public class UserSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition> {
	public UserSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "users";
	public override string Description => "See who are opted in to the graveyard.";
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) => Task.FromResult(builder);
}