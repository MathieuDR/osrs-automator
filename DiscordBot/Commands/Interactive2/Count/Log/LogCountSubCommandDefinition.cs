namespace DiscordBot.Commands.Interactive2.Count.Log; 

public class LogCountSubCommandDefinition : SubCommandDefinitionBase<CountRootCommandDefinition> {
    public LogCountSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
    public override string Name => "log";
    public override string Description => "get a log of your or somebody else their score history";
    public static string UserOption => "user";
    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(UserOption, ApplicationCommandOptionType.User, "the user to check", false);
        return Task.FromResult(builder);
    }
}