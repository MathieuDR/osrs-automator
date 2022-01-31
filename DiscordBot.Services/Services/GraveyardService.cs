using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Services; 

internal class GraveyardService: IGraveyardService {
	//private readonly IGraveyardRepository _graveyardRepository;
	private readonly IRepositoryStrategy _repositoryStrategy;
	private readonly ILogger<GraveyardService> _logger;

	public GraveyardService(IRepositoryStrategy repositoryStrategy, ILogger<GraveyardService> logger) {
		_repositoryStrategy = repositoryStrategy;
		_logger = logger;
	}
	
	public Task<Result> OptIn(GuildUser user) {
		_logger.LogInformation($"Opting in user {user.Username}");
		
		var repository = _repositoryStrategy.GetOrCreateRepository<IGraveyardRepository>(user.GuildId);

		var configurationResult = repository.GetSingle();
		if (configurationResult.IsFailed) {
			return Task.FromResult(configurationResult.ToResult());
		}

		Graveyard configuration = configurationResult.Value ?? new Graveyard();
		
		if (configuration.OptedInUsers.Any(x => x == user.Id)) {
			return Task.FromResult(Result.Ok());
		}
		
		configuration.OptedInUsers.Add(user.Id);
		var repoResult = repository.UpdateOrInsert(configuration);
		
		return Task.FromResult(Result.OkIf(repoResult.IsSuccess, "Could not opt in").WithErrors(repoResult.Errors));
	}

	public Task<Result> OptOut(GuildUser user) {
		_logger.LogInformation($"Opting user {user.Username} out");
		
		var graveyardRepository = _repositoryStrategy.GetOrCreateRepository<IGraveyardRepository>(user.GuildId);
		
		var configurationResult = graveyardRepository.GetSingle();
		if (configurationResult.IsFailed || configurationResult.Value == null) {
			return Task.FromResult(Result.Fail("Could not opt user out")
				.WithErrors(configurationResult.Errors));
		}
		
		if (configurationResult.Value.OptedInUsers.All(x => x != user.Id)) {
			return Task.FromResult(Result.Ok());
		}

		configurationResult.Value.OptedInUsers.Remove(user.Id);
		var repoResult = graveyardRepository.Update(configurationResult.Value);
		return Task.FromResult(Result.OkIf(repoResult.IsSuccess, "Could not opt out").WithErrors(repoResult.Errors));
	}

	public Task<Result<bool>> IsOptedIn(GuildUser user) {
		var graveyardRepository = _repositoryStrategy.GetOrCreateRepository<IGraveyardRepository>(user.GuildId);
		var configurationResult = graveyardRepository.GetSingle();
		
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
		var shamerOptedIn = await IsOptedIn(shamedBy);
		if (!shamerOptedIn.Value) {
			return Result.Fail("User that is shaming is not opted in, please opt in to shame.");
		}
		
		var isOptedIn = await IsOptedIn(shamed);

		if (!isOptedIn.Value) {
			return Result.Fail("User that is being shamed is not opted in.");
		}
		
		var shame = new Shame(location, metricType, imageUrl, shamedBy.Id);
		var graveyardRepository = _repositoryStrategy.GetOrCreateRepository<IGraveyardRepository>(shamed.GuildId);
		return graveyardRepository.AddShame(shamed.Id, shame);
	}

	public async Task<Result<IEnumerable<Shame>>> GetShames(GuildUser user, ShameLocation? location, MetricType? metricTypeLocation) {
		var isOptedIn = await IsOptedIn(user);

		if (!isOptedIn.Value) {
			return Result.Fail("User is not opted in.");
		}
		
		var graveyardRepository = _repositoryStrategy.GetOrCreateRepository<IGraveyardRepository>(user.GuildId);
		var repositoryResult = location is null ? graveyardRepository.GetShamesForUser(user.Id) 
			: graveyardRepository.GetShamesForUserPerLocation(user.Id, location.Value, metricTypeLocation);
		
		if(repositoryResult.IsFailed){
			return Result.Fail<IEnumerable<Shame>>("Could not get shames")
				.WithErrors(repositoryResult.Errors);
		}
		
		return Result.Ok(SetTimezone(repositoryResult.Value, user.GuildId));
	}

	public Task<Result<(ulong userId, Shame[] shames)[]>> GetShames(Guild guild, ShameLocation? location, MetricType? metricTypeLocation) {
		var graveyardRepository = _repositoryStrategy.GetOrCreateRepository<IGraveyardRepository>(guild.GuildId);
		var graveyard = graveyardRepository.GetSingle();
		
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
		
		var filteredShamesPerLocation = filteredShames
			.Where(x => x.Item2.Any(y => y.Location == location.Value && (metricTypeLocation is null || y.MetricLocation == metricTypeLocation.Value)))
			.Select(x=> (x.Key, SetTimezone(x.Item2, guild.Id).ToArray())).ToArray();
		return Task.FromResult(Result.Ok(filteredShamesPerLocation));
	}

	private IEnumerable<Shame> SetTimezone(IEnumerable<Shame> shames, ulong guildId) {
		var configRepo = _repositoryStrategy.GetOrCreateRepository<IGuildConfigRepository>(guildId);
		var configuration = configRepo.GetSingle().Value;

		return shames.Select(x => x with { ShamedAt = x.ShamedAt.ToOffset(configuration.Timezone)});
	}
}
