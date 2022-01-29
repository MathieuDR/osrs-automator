using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Commands.Interactive2.Graveyard.Helpers;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shame;

public class ShameCommandHandler : ApplicationCommandHandlerBase<ShameCommandRequest> {
	private readonly IGraveyardService _graveyardService;
	private readonly MetricTypeParser _metricTypeParses;

	public ShameCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
		_metricTypeParses = serviceProvider.GetRequiredService<MetricTypeParser>();
	}

	private async Task<Result<(List<IUser>, ShameLocation location, MetricType? metricType, string imageUrl)>> GetOptions() {
		var shamedUserString = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.ShamedOption);
		var locationString = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.LocationOption);
		var pictureUrl = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.PictureOption);

		var users = (await shamedUserString.GetUsersFromString(Context)).users.ToList();
		
		// Validate at least one user
		if (users.Count == 0) {
			return Result.Fail("No users found");
		}

		// Validate URL
		pictureUrl = !Uri.TryCreate(pictureUrl, UriKind.Absolute, out var pictureUri) ? null : pictureUri.AbsoluteUri;

		var (location, metricType) = ShameExtensions.ToLocation(locationString, _metricTypeParses);
		return Result.Ok((users, location, metricType, pictureUrl));
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var optionsResult = await GetOptions();
		if(optionsResult.IsFailed) {
			return Result.Fail("Could not get command values").WithErrors(optionsResult.Errors);
		}
		
		var (users, location, metricType, pictureUrl) = optionsResult.Value;

		var serviceResult = await SendToService(users, location, pictureUrl, metricType);
		await ReplyAsync(serviceResult.IsSuccess, users.Count);
		
		return serviceResult;
	}

	private async Task ReplyAsync(bool isSuccess, int usersCount) {
		var embed = Context.CreateEmbedBuilder();
		if (isSuccess) {
			embed.WithSuccess($"Shamed {usersCount} users!");
		} else {
			embed.WithFailure($"Could not shame {usersCount} users!");
		}

		await Context.CreateReplyBuilder().WithEmbed(embed).RespondAsync();
	}

	private async Task<Result> SendToService(List<IUser> users, ShameLocation location, string pictureUrl, MetricType? metricType) {
		var tasks = new Task<Result>[users.Count];
		for (var i = 0; i < users.Count; i++) {
			var user = users[i];
			tasks[i] = _graveyardService.Shame(user.ToGuildUserDto(), location, pictureUrl, metricType);
		}

		await Task.WhenAll(tasks);
		var failedTasks = tasks.Select((result, i) => (i, result.Result)).Where(t => t.Result.IsFailed).ToList();

		// No failed tasks
		if (!failedTasks.Any()) {
			return Result.Ok();
		}

		// If we failed, at least tell em!
		var failedUsers = failedTasks.Select(t => $"{users[t.i].Mention}: {t.Result.Errors.First().Message}").ToList();
		return Result.Fail($"Failed to shame the following users: {string.Join(", ", failedUsers)}").WithErrors(failedTasks.SelectMany(x=> x.Result.Errors));
	}
}
