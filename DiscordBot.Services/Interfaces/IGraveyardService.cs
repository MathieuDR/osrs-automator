using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Graveyard;
using DiscordBot.Common.Models.Enums;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Interfaces; 

public interface IGraveyardService {
	Task<Result> OptIn(GuildUser user);
	Task<Result> OptOut(GuildUser user);
	Task<Result<bool>> IsOptedIn(GuildUser user);
	Task<Result> Shame(GuildUser shamed, GuildUser shamedBy, ShameLocation location, string imageUrl, MetricType? metricType);
	Task<Result<IEnumerable<Shame>>> GetShames(GuildUser user, ShameLocation? location, MetricType? metricTypeLocation);
	Task<Result<(DiscordUserId userId, Shame[] shames)[]>> GetShames(Guild guild, ShameLocation? location, MetricType? metricTypeLocation);
	Task<Result<DiscordUserId[]>> GetOptedInUsers(Guild guild);
	Task<Result> RemoveShame(GuildUser user, Guid id);
}
