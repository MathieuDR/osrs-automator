using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Ping;


public class PingRootCommandDefinition : RootCommandDefinitionBase {
    public override string Name => "ping2";
    public override string Description => "Ping command through mediatr";

    public override Guid Id => Guid.Parse("341A00F5-AB4A-451F-8FA4-639D54EE658C");

    protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) {
        return Task.FromResult<SlashCommandBuilder>(builder);
    }

    public PingRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
}

public class InsultSubCommandDefinition : SubCommandDefinitionBase<PingRootCommandDefinition> {
    public override string Name => "insult";
    public  override string Description => "Receive an insult";
    public string VariantOption => "Variant";
    
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
