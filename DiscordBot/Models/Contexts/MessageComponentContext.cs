using System.Collections.Immutable;
using DiscordBot.Common.Identities;

namespace DiscordBot.Models.Contexts;

public class MessageComponentContext : BaseInteractiveContext<SocketMessageComponent> {
    public MessageComponentContext(SocketMessageComponent innerContext, IServiceProvider provider)
        : base(innerContext, provider) { }

    public string CustomId => InnerContext.Data.CustomId;
    public string[] Parameters => CustomId.Split(':').Last().Split('.');
    public string[] CustomIdParts => CustomId.Split(':').First().Split('.');
    public string CustomSubCommandId => CustomIdParts?[1];

    public IEnumerable<string> SelectedMenuOptions
        => InnerContext.Data.Values?.ToHashSet() ?? new HashSet<string>();

    public IReadOnlyCollection<EmbedField> EmbedFields => InnerContext.Message.Embeds.FirstOrDefault()?.Fields ?? new ImmutableArray<EmbedField>();

    public override string Message => InnerContext.Message.Content;
    public DiscordMessageId MessageId => new DiscordMessageId(InnerContext.Message.Id);

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

    public override string Command => CustomIdParts.FirstOrDefault();
    public override string SubCommand => CustomIdParts.Length > 1 ? CustomIdParts[1] : null;
    public override string SubCommandGroup => null;
}
