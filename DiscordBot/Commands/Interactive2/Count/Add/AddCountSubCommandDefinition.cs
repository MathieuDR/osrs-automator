namespace DiscordBot.Commands.Interactive2.Count.Add; 

public class AddCountSubCommandDefinition :SubCommandDefinitionBase<CountRootCommandDefinition>{
    public AddCountSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
    public override string Name => "add";
    public override string Description => "Add a score to one or multiple users";

    public static string ValueOption => "score";
    public static string UsersOption => "users";
    public static string ReasonOption => "reason";

    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(ValueOption, ApplicationCommandOptionType.Integer, "The score to be added or subtracted", true);
        builder.AddOption(UsersOption, ApplicationCommandOptionType.String, "The user(s) where the score applies", true);
        builder.AddOption(ReasonOption, ApplicationCommandOptionType.String, "Why does the score apply", true);
        return Task.FromResult(builder);
    }
}