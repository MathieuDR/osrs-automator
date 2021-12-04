namespace DiscordBot.Models.Contexts;

public class AutocompleteCommandContext : BaseInteractiveContext<SocketAutocompleteInteraction> {
    public AutocompleteCommandContext(SocketAutocompleteInteraction interaction, IServiceProvider provider) : base(interaction, provider) { }

    public string CurrentOptionAsString => Current.Value.ToString();

    public AutocompleteOption Current => InnerContext.Data.Current;
    
    public override string Command => InnerContext.Data.CommandName;

    public override string SubCommand => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand).Select(x => x.Name)
        .FirstOrDefault();

    public override string SubCommandGroup => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommandGroup).Select(x => x.Name)
        .FirstOrDefault();
   
    public string CurrentOption => InnerContext.Data.Options.Where(x => x.Value == Current.Value).Select(x => x.Name).FirstOrDefault();

    public string CommandFullName => string.IsNullOrWhiteSpace(SubCommand) ? Command : $"{Command} {SubCommand}";
    public override string Message => $"Autocompleting '{Current}' for {CurrentOption} in {CommandFullName}";
    
    public Task RespondAsync<T>(IEnumerable<T> options) {
        return InnerContext.RespondAsync(options?.Take(20).Select(x => new AutocompleteResult(x.ToString(), x.ToString()?.ToLower())));
    }

    public override Task RespondAsync(string text = null, IEnumerable<Embed> embeds = null, bool isTts = false, bool ephemeral = false, AllowedMentions allowedMentions = null,
        RequestOptions options = null, MessageComponent component = null) {
        throw new InvalidOperationException("Please send options using the correct method");
    }
}
