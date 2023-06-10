namespace DiscordBot.Commands.Interactive2.Confirm.Configure; 

internal sealed class ConfigureConfirmCommandDefinition : SubCommandDefinitionBase<ConfirmRootCommandDefinition>{
    public ConfigureConfirmCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
    public override string Name => "configure";
    public override string Description => "Configure the confirmation workflow";
    public static string ConfirmChannel => "confirm-channel";
    
    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(ConfirmChannel, ApplicationCommandOptionType.Channel, "Channel for the confirmation messages", true);
        return Task.FromResult(builder);
    }
}
