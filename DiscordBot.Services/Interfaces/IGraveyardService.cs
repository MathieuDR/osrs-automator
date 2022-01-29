using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Interfaces; 

public interface IGraveyardService {
	Task<Result> OptIn(GuildUser user);
	Task<Result> OptOut(GuildUser user);
	Task<Result> Shame(GuildUser user, ShameLocation location, string imageUrl, MetricType? metricType);
	Task<Result<IEnumerable<Shame>>> GetShames(GuildUser user, ShameLocation? location, MetricType? metricTypeLocation);
	Task<Result<IEnumerable<Shame>>> GetShames(Guild guild, ShameLocation? location, MetricType? metricTypeLocation);
}
