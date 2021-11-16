using Common;

namespace DiscordBot.Models.Contexts;

public class AutocompleteCommandContext : BaseInteractiveContext<SocketAutocompleteInteraction> {
    public AutocompleteCommandContext(SocketAutocompleteInteraction interaction, IServiceProvider provider) : base(interaction, provider) { }

    public string CurrentOptionAsString => Current.ToString();

    public AutocompleteOption Current => InnerContext.Data.Current;

    public string CommandName => InnerContext.Data.CommandName;

    public string SubCommand => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand).Select(x => x.Name).FirstOrDefault();

    public string CurrentOption => InnerContext.Data.Options.Where(x => x.Value == Current.Value).Select(x => x.Name).FirstOrDefault();

    //public string Option => InnerContext.;

    public Task RespondWithOptions<T>(IEnumerable<T> options) {
        return InnerContext.RespondAsync(options.Select(x => new AutocompleteResult(x.ToString(), x.ToString()?.ToLower())));
    }

    public string CommandFullName => string.IsNullOrWhiteSpace(SubCommand) ? CommandName : $"{CommandName} {SubCommand}";
    public override string Message => $"Autocompleting '{Current}' for {CurrentOption} in {CommandFullName}";
}
