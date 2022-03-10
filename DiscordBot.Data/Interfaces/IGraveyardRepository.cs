using DiscordBot.Common.Models.Data.Graveyard;
using DiscordBot.Common.Models.Enums;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Data.Interfaces; 

public interface IGraveyardRepository : ISingleRecordRepository<Graveyard> {
	public Result<List<Shame>> GetShamesForUser(DiscordUserId userId);
	public Result<Dictionary<DiscordUserId, List<Shame>>> GetShamesPerLocation(ShameLocation location, MetricType? metricTypeLocation = null);
	public Result<List<Shame>> GetShamesForUserPerLocation(DiscordUserId userId, ShameLocation location, MetricType? metricTypeLocation = null);
	public Result AddShame(DiscordUserId userId, Shame shame);
	public Result RemoveShame(DiscordUserId userId, Guid shameId);
}
