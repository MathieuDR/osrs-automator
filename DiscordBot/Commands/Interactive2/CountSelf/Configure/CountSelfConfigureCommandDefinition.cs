namespace DiscordBot.Commands.Interactive2.CountSelf.Configure; 

internal sealed class CountSelfConfigureCommandDefinition : SubCommandDefinitionBase<CountSelfRootCommandDefinition> {
    public CountSelfConfigureCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
    public override string Name => "configure";
    public override string Description => "Configure the self count workflow";

    public static string Channel => "request-channel";
    public static string Json => "json";
    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(Channel, ApplicationCommandOptionType.Channel, "Channel for the self count requests", false);
        builder.AddOption(Json, ApplicationCommandOptionType.Boolean, "The items to parse", false);
        return Task.FromResult(builder);
    }
}
