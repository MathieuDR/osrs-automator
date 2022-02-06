using System.Text;
using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Graveyard.Users; 

public class UserSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition> {
	public UserSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "users";
	public override string Description => "See who are opted in to the graveyard.";
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) => Task.FromResult(builder);
}

public class UserSubCommandRequest : ApplicationCommandRequestBase<UserSubCommandDefinition> {
	public UserSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanMember;
}

public class UserSubCommandHandler : ApplicationCommandHandlerBase<UserSubCommandRequest> {
	private readonly IGraveyardService _graveyardService;

	public UserSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var users = await _graveyardService.GetOptedInUsers(Context.Guild.ToGuildDto());

		if (users.IsFailed) {
			return users.ToResult();
		}
		
		// Create a string builder to build the message in pages
		await PresentUsers(users.Value);
		
		return Result.Ok();
	}

	private Task PresentUsers(ulong[] usersValue) {
		var strings = usersValue.Select(x=> $"<@{x}>").ToArray();
		// Send the message
		_ = Context.CreatePaginatorReplyBuilder().WithLines(strings, modifications: x => x.WithTitle("Opted in to the graveyard")).RespondAsync();
		
		return Task.CompletedTask;
	}
}
