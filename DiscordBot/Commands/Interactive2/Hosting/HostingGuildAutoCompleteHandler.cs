namespace DiscordBot.Commands.Interactive2.Hosting;

public class HostingGuildAutoCompleteHandler : AutoCompleteHandlerBase<HostingGuildAutoCompleteRequest> {
	public HostingGuildAutoCompleteHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var q = Context.CurrentOptionAsString ?? "";
		var options = Context.Client.Guilds
			.Where(g => g.Name.Contains(q, StringComparison.OrdinalIgnoreCase))
			.OrderBy(g => g.Name)
			.Take(20)
			.Select(g => (g.Name, g.Id.ToString()));

		_ = Context.RespondAsync(options);
		return Result.Ok();
	}
}
