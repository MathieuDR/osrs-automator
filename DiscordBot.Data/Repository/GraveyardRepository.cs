using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Data.Repository; 

public class GraveyardRepository :BaseRecordLiteDbRepository<Graveyard>, IGraveyardRepository{
	public GraveyardRepository(ILogger logger, LiteDatabase database) : base(logger, database) { }
	public override string CollectionName => "graveyard";
	
	public Task<Result<List<Shame>>> GetShamesForUser(ulong userId) => throw new NotImplementedException();

	public Task<Result<Dictionary<ulong, List<Shame>>>> GetShamesPerLocation(ShameLocation location, MetricType? metricTypeLocation = null) => throw new NotImplementedException();

	public Task<Result<List<Shame>>> GetShamesForUserPerLocation(ulong userId, ShameLocation location, MetricType? metricTypeLocation = null) => throw new NotImplementedException();
}
