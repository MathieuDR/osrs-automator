using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Services; 

internal class GraveyardService: IGraveyardService {
	private readonly IGraveyardRepository _graveyardRepository;
	private readonly ILogger<GraveyardService> _logger;

	public GraveyardService(IGraveyardRepository graveyardRepository, ILogger<GraveyardService> logger) {
		_graveyardRepository = graveyardRepository;
		_logger = logger;
	}
	
	public Task<Result> OptIn(GuildUser user) {
		_logger.LogInformation($"Opting in user {user.Username}");

		var configurationResult = _graveyardRepository.GetSingle();
		if (configurationResult.IsFailed) {
			return Task.FromResult(configurationResult.ToResult());
		}

		Graveyard configuration = configurationResult.Value ?? new Graveyard();
		
		if (configuration.OptedInUsers.Any(x => x == user.Id)) {
			return Task.FromResult(Result.Ok());
		}
		
		configuration.OptedInUsers.Add(user.Id);
		var repoResult = _graveyardRepository.UpdateOrInsert(configuration);
		
		return Task.FromResult(Result.OkIf(repoResult.IsSuccess, "Could not opt in").WithErrors(repoResult.Errors));
	}

	public Task<Result> OptOut(GuildUser user) {
		_logger.LogInformation($"Opting user {user.Username} out");
		
		var configurationResult = _graveyardRepository.GetSingle();
		if (configurationResult.IsFailed || configurationResult.Value == null) {
			return Task.FromResult(Result.Fail("Could not opt user out")
				.WithErrors(configurationResult.Errors));
		}
		
		if (configurationResult.Value.OptedInUsers.All(x => x != user.Id)) {
			return Task.FromResult(Result.Ok());
		}

		configurationResult.Value.OptedInUsers.Remove(user.Id);
		var repoResult = _graveyardRepository.Update(configurationResult.Value);
		return Task.FromResult(Result.OkIf(repoResult.IsSuccess, "Could not opt out").WithErrors(repoResult.Errors));
	}

	public Task<Result<bool>> IsOptedIn(GuildUser user) {
		var configurationResult = _graveyardRepository.GetSingle();
		
		if(configurationResult.IsFailed){
			return Task.FromResult(Result.Fail<bool>("Could not check if user is opted in")
				.WithErrors(configurationResult.Errors));
		}
		
		if (configurationResult.Value == null) {
			return Task.FromResult(Result.Ok(false));
		}

		return Task.FromResult(Result.Ok(configurationResult.Value.OptedInUsers.Any(x => x == user.Id)));
	}

	public async Task<Result> Shame(GuildUser shamed, GuildUser shamedBy, ShameLocation location, string imageUrl, MetricType? metricType) {
		var isOptedIn = await IsOptedIn(shamed);

		if (!isOptedIn.Value) {
			return Result.Fail("User that is being shamed is not opted in.");
		}

		var shamerOptedIn = await IsOptedIn(shamedBy);
		if (!shamerOptedIn.Value) {
			return Result.Fail("User that is shaming is not opted in, please opt in to shame.");
		}
		
		var shame = new Shame(location, metricType, imageUrl, shamedBy.Id, DateTimeOffset.Now); //TODO Get offset!
		return _graveyardRepository.AddShame(shamed.Id, shame);

	}

	public async Task<Result<IEnumerable<Shame>>> GetShames(GuildUser user, ShameLocation? location, MetricType? metricTypeLocation) {
		var isOptedIn = await IsOptedIn(user);

		if (!isOptedIn.Value) {
			return Result.Fail("User is not opted in.");
		}
		
		Result<List<Shame>> repositoryResult;		
		
		if (location is null) {
			repositoryResult = _graveyardRepository.GetShamesForUser(user.Id);
		} else {
			repositoryResult = _graveyardRepository.GetShamesForUserPerLocation(user.Id, location.Value, metricTypeLocation);
		}
		
		if(repositoryResult.IsFailed){
			return Result.Fail<IEnumerable<Shame>>("Could not get shames")
				.WithErrors(repositoryResult.Errors);
		}
		
		return Result.Ok(repositoryResult.Value.AsEnumerable());
	}

	public Task<Result<(ulong userId, Shame[] shames)[]>> GetShames(Guild guild, ShameLocation? location, MetricType? metricTypeLocation) {
		var graveyard = _graveyardRepository.GetSingle();
		
		if(graveyard.IsFailed || graveyard.Value is null){
			return Task.FromResult(Result.Fail<(ulong userId, Shame[] shames)[]>("Could not get shames")
				.WithErrors(graveyard.Errors));
		}

		var shamesPerUse = graveyard.Value.Shames;
		
		// filter shames for people	who are opted in
		var optedInUsers = graveyard.Value.OptedInUsers;
		var filteredShames = shamesPerUse.Where(x => optedInUsers.Contains(x.Key)).Select(x => (x.Key, x.Value.ToArray())).ToArray();
		
		// filter shames for location
		if (location is null) {
			return Task.FromResult(Result.Ok(filteredShames));
		}
		
		var filteredShamesPerLocation = filteredShames.Where(x => x.Item2.Any(y => y.Location == location.Value && (metricTypeLocation is null || y.MetricLocation == metricTypeLocation.Value))).ToArray();
		return Task.FromResult(Result.Ok(filteredShamesPerLocation));
	}
}
