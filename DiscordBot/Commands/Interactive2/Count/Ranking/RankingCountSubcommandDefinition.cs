namespace DiscordBot.Commands.Interactive2.Count.Ranking;

public class RankingCountSubcommandDefinition : SubCommandDefinitionBase<CountRootCommandDefinition> {
    public RankingCountSubcommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
    public override string Name => "ranking";
    public override string Description => "See the leaderboards of you or an user";
    public static string UserOption => "user";

    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(UserOption, ApplicationCommandOptionType.User, "See the ranking of specific user", false);
        return Task.FromResult(builder);
    }
}