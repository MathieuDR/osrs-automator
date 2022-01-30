using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Data.Interfaces; 

public interface IGraveyardRepository : IRecordRepository<Graveyard> {
	public Task<Result<List<Shame>>> GetShamesForUser(ulong userId);
	// public Task<IEnumerable<Shame>> GetShamesByUser(ulong userId);
	public Task<Result<Dictionary<ulong, List<Shame>>>> GetShamesPerLocation(ShameLocation location, MetricType? metricTypeLocation = null);
	public Task<Result<List<Shame>>> GetShamesForUserPerLocation(ulong userId, ShameLocation location, MetricType? metricTypeLocation = null);
	
}
