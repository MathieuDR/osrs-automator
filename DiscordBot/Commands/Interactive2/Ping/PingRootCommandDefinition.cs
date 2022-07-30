namespace DiscordBot.Commands.Interactive2.Ping;


public class PingRootCommandDefinition : RootCommandDefinitionBase {
    public override string Name => "ping2";
    public override string Description => "Ping command through mediatr";

    protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) {
        return Task.FromResult<SlashCommandBuilder>(builder);
    }

    public PingRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
}