using System.Diagnostics.Contracts;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Edit; 

public class EditShameSubCommandHandler : ApplicationCommandHandlerBase<EditShameSubCommandRequest> {
	private readonly IGraveyardService _graveyardService;
	private readonly MetricTypeParser _metricTypeParses;
	public EditShameSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
		_metricTypeParses = serviceProvider.GetRequiredService<MetricTypeParser>();
	}
	protected override Task<Result> DoWork(CancellationToken cancellationToken) {
		var optionsResult = GetOptions();

		if (optionsResult.IsFailed) {
			_ = Context.CreateReplyBuilder(true).FromResult(optionsResult.ToResult()).RespondAsync();
			return Task.FromResult(Result.Fail("Could not get options").WithErrors(optionsResult.Errors));
		}

		var (user, shameId, imageUrl, location, metricType) = optionsResult.Value;

		if (location is not null) {
			_graveyardService.UpdateShameLocation(user.ToGuildUserDto(), shameId, location.Value, metricType);
		}

		if (!string.IsNullOrWhiteSpace(imageUrl)) {
			_graveyardService.UpdateShameImage(user.ToGuildUserDto(), shameId, imageUrl);
		}
		
		_ = Context.CreateReplyBuilder(true).WithEmbed(x=> x.WithSuccess("Shame updated!")).RespondAsync();
		return Task.FromResult(Result.Ok());
	}

	private Result<(IUser user, Guid shameId, string? imageUrl, ShameLocation? location, MetricType? metricType )> GetOptions() {
		var user = Context.SubCommandOptions.GetOptionValue<IUser>(EditShameSubcommandDefinition.ShamedOption);
		var id = Context.SubCommandOptions.GetOptionValue<string>(EditShameSubcommandDefinition.ShameId);

		if (user is null) {
			return Result.Fail("User not found, is null.");
		}

		if (String.IsNullOrWhiteSpace(id)) {
			return Result.Fail("Id empty or null.");
		}

		if (!Guid.TryParse(id, out var parsedId)) {
			return Result.Fail("Id is not a valid Guid.");
		}


		var locationString = Context.SubCommandOptions.GetOptionValue<string>(EditShameSubcommandDefinition.LocationOption);
		var pictureUrl = Context.SubCommandOptions.GetOptionValue<string>(EditShameSubcommandDefinition.PictureOption);

		if (!string.IsNullOrWhiteSpace(pictureUrl)) {
			// Validate URL
			pictureUrl = !Uri.TryCreate(pictureUrl, UriKind.Absolute, out var pictureUri) ? null : pictureUri.AbsoluteUri;
		}

		ShameLocation? location = null;
		MetricType? metricType = null;
		
		if (!string.IsNullOrWhiteSpace(locationString)) {
			(location, metricType) = ShameExtensions.ToLocation(locationString, _metricTypeParses);	
		}

		return Result.Ok<(IUser user, Guid shameId, string? imageUrl, ShameLocation? location, MetricType? metricType )>((user, parsedId, pictureUrl, location, metricType));
	}
	
}
