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

	public Task<Result> OptOut(GuildUser user) => throw new NotImplementedException();
	public Task<Result<bool>> IsOptedIn(GuildUser user) => throw new NotImplementedException();

	public Task<Result> Shame(GuildUser shamed, GuildUser shamedBy, ShameLocation location, string imageUrl, MetricType? metricType) => throw new NotImplementedException();

	public Task<Result<IEnumerable<Shame>>> GetShames(GuildUser user, ShameLocation? location, MetricType? metricTypeLocation) => throw new NotImplementedException();

	public Task<Result<IEnumerable<Shame>>> GetShames(Guild guild, ShameLocation? location, MetricType? metricTypeLocation) => throw new NotImplementedException();
}
