using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Ping;


public class PingRootCommandDefinition : RootCommandDefinitionBase {
    public override string Name => "ping2";
    public override string Description => "Ping command through mediatr";

    protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) {
        return Task.FromResult<SlashCommandBuilder>(builder);
    }

    public PingRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
}

public class NormalSubCommandDefinition : SubCommandDefinitionBase<PingRootCommandDefinition> {
    public override string Name => "normal";
    public  override string Description => "Get a nice message";
    
    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        return Task.FromResult(builder);
    }

    public NormalSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
}

public class InsultSubCommandDefinition : SubCommandDefinitionBase<PingRootCommandDefinition> {
    public override string Name => "insult";
    public  override string Description => "Receive an insult";
    public string VariantOption => "variant";
    
    protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
        builder.AddOption(VariantOption, ApplicationCommandOptionType.Boolean,"Variant of insult", false);
        return Task.FromResult(builder);
    }

    protected override Task FillOptions() {
        var optionList = Options.ToList();
        optionList.Add((VariantOption, typeof(bool)));
        
        return base.FillOptions();
    }

    public InsultSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
}
