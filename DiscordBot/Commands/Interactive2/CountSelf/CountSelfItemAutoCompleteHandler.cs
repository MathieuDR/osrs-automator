using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.CountSelf;

internal sealed  class CountSelfItemAutoCompleteHandler : AutoCompleteHandlerBase<CountSelfItemAutoCompleteRequest> {
    private ICounterService _counterService;

    public CountSelfItemAutoCompleteHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _counterService = serviceProvider.GetRequiredService<ICounterService>();
    }

    protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
        // get guild from context
        // get current option as string

        (var guild, var currentOption) = (Context.Guild.ToGuildDto(), Context.CurrentOptionAsString);

        // See if we can find option from option in guild
        var options = await _counterService.GetItemsForGuild(guild, currentOption);
        

        _ = Context.RespondAsync(options.Select(x=> (x.item.Name, x.item.Name)));

        return Result.Ok();
    }
}
