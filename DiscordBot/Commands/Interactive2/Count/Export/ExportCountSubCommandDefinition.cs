namespace DiscordBot.Commands.Interactive2.Count.Export; 

public class ExportCountSubCommandDefinition : SubCommandDefinitionBase<CountRootCommandDefinition> {
    public ExportCountSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
    public override string Name => "export";
    public override string Description => "export all counts with logs";
    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) => Task.FromResult(builder);
}