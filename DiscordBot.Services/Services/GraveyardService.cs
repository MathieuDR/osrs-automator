using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Services.Interfaces;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Services; 

internal class GraveyardService: IGraveyardService {
	public GraveyardService() {
		
	}
	public Task<Result> OptIn(GuildUser user) => throw new NotImplementedException();

	public Task<Result> OptOut(GuildUser user) => throw new NotImplementedException();
	public Task<Result<bool>> IsOptedIn(GuildUser user) => throw new NotImplementedException();

	public Task<Result> Shame(GuildUser shamed, GuildUser shamedBy, ShameLocation location, string imageUrl, MetricType? metricType) => throw new NotImplementedException();

	public Task<Result<IEnumerable<Shame>>> GetShames(GuildUser user, ShameLocation? location, MetricType? metricTypeLocation) => throw new NotImplementedException();

	public Task<Result<IEnumerable<Shame>>> GetShames(Guild guild, ShameLocation? location, MetricType? metricTypeLocation) => throw new NotImplementedException();
}
