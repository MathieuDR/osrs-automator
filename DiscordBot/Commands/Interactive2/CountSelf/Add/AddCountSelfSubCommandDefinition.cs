namespace DiscordBot.Commands.Interactive2.CountSelf.Add; 

internal sealed class AddCountSelfSubCommandDefinition :SubCommandDefinitionBase<CountSelfRootCommandDefinition>{
    public AddCountSelfSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
    public override string Name => "add";
    public override string Description => "Add a score to one or multiple users";
    public static string ItemOption => "item";
    public static string SplitUsers  => "splitting";
    public static string Image  => "image";

    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(ItemOption, ApplicationCommandOptionType.String, "The item you received!", true, isAutocomplete: true);
        builder.AddOption(Image, ApplicationCommandOptionType.Attachment, "The image", true);
        builder.AddOption(SplitUsers, ApplicationCommandOptionType.String, "The user(s) where you split it with", false);
        return Task.FromResult(builder);
    }
}