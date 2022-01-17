using DiscordBot.Commands.Interactive2.Base.Handlers;

namespace DiscordBot.Commands.Interactive2.Graveyard.OptIn;

public class OptInCommandHandler: ApplicationCommandHandlerBase<OptInCommandRequest> {
	public OptInCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
	protected override Task<Result> DoWork(IEnumerable<(string optionName, Type optionType, object? optionValue)> options) => throw new NotImplementedException();
}
