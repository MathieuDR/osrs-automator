using DiscordBot.Commands.Interactive2.Base.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Funds.Manage;

public class ManageFundsSubCommandHandler: ApplicationCommandHandlerBase<ManageFundsSubCommandRequest> {
	private readonly IClanFundsService _clanFundsService;

	public ManageFundsSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_clanFundsService = serviceProvider.GetRequiredService<IClanFundsService>();
	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var (user, authority, type, amount, reason) = GetOptions();

		if (type == ClanFundEventType.Refund || type == ClanFundEventType.Withdraw) {
			if (amount > 0) {
				amount *= -1;
			}
		}

		var fundEvent = new ClanFundEvent(user.Id, authority.Id, reason, user.Username, amount, type);
		var result = await _clanFundsService.AddClanFund(Context.Guild.ToGuildDto(), fundEvent);

		if (result.IsSuccess) {
			_ = Context.CreateReplyBuilder()
				.WithEmbed(x => x.WithSuccess("Event added"))
				.WithEphemeral(true)
				.RespondAsync();
		}
		
		return Result.OkIf(result.IsSuccess, "Could not save the event").WithErrors(result.Errors);
	}

	private (IUser from, IUser authority, ClanFundEventType type, long amount, string? reason) GetOptions() {
		var user = Context.SubCommandOptions.GetOptionValue<IUser>(ManageFundsSubCommandDefinition.Player);
		var amount = Context.SubCommandOptions.GetOptionValue<string>(ManageFundsSubCommandDefinition.AmountOption).ToLong();
		var reason = Context.SubCommandOptions.GetOptionValue<string>(ManageFundsSubCommandDefinition.Reason);
		var option = Context.SubCommandOptions.GetOptionValueAsEnum<ClanFundEventType>(ManageFundsSubCommandDefinition.TypeOption);
		
		return (user, Context.User, option, amount, reason);
	}
}