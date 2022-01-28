using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Services.Interfaces;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Services; 

internal class GraveyardService: IGraveyardService {
	public Task<Result> OptIn(GuildUser user) => Task.FromResult( Result.Ok());

	public Task<Result> OptOut(GuildUser user) =>  Task.FromResult( Result.Ok());

	public Task<Result> Shame(GuildUser user, ShameLocation location, string imageUrl, MetricType? metricType) => throw new NotImplementedException();

	public Task<Result<Shame>> GetShames(GuildUser user, ShameLocation? location, MetricType? metricTypeLocation) => throw new NotImplementedException();

	public Task<Result<Shame>> GetShames(Guild guild, ShameLocation? location, MetricType? metricTypeLocation) => throw new NotImplementedException();
}
