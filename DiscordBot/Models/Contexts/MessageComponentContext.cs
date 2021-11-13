using System.Collections.Immutable;

namespace DiscordBot.Models.Contexts; 

public class MessageComponentContext : BaseInteractiveContext<SocketMessageComponent> {
    public MessageComponentContext(SocketMessageComponent innerContext, IServiceProvider provider)
        : base(innerContext, provider) { }

    public string CustomId => InnerContext.Data.CustomId;
    public string[] CustomIdParts => CustomId.Split('.');
    public string CustomSubCommandId => CustomIdParts?[1];

    public IEnumerable<string> SelectedMenuOptions
        => InnerContext.Data.Values?.ToHashSet() ?? new HashSet<string>();

    public IReadOnlyCollection<EmbedField> EmbedFields => this.InnerContext.Message.Embeds.FirstOrDefault()?.Fields ?? new ImmutableArray<EmbedField>();

    public Task UpdateAsync(
        Optional<string> content = new(),
        Optional<Embed> embed = new(),
        Optional<Embed[]> embeds = new(),
        Optional<AllowedMentions> allowedMentions = new(),
        Optional<MessageComponent> component = new(),
        Optional<MessageFlags?> flags = new(),
        RequestOptions options = null
    ) {
        return InnerContext.UpdateAsync(props => {
            props.AllowedMentions = allowedMentions;
            props.Embeds = embeds;
            props.Embed = embed;
            props.Content = content;
            props.Components = component.Value is null ? new ComponentBuilder().Build() : component;
            props.Flags = flags;
        }, options);
    }
        
    public override string Message => InnerContext.Message.Content;
}