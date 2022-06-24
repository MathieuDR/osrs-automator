using DiscordBot.Common.Models.Data.Graveyard;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Data.Repository; 

internal class GraveyardRepository : BaseSingleRecordLiteDbRepository<Graveyard>, IGraveyardRepository{
	public GraveyardRepository(ILogger logger, LiteDatabase database) : base(logger, database) { }
	public override string CollectionName => "graveyard";
	
	public Result<List<Shame>> GetShamesForUser(DiscordUserId userId) {
		var graveyard = GetGraveyardOrFail();
		if (graveyard.IsFailed) {
			return graveyard.ToResult();
		}

		var shames = graveyard.Value.Shames.FirstOrDefault(s => s.Key == userId).Value;
		return Result.Ok(shames);
	}

	public Result<Dictionary<DiscordUserId, List<Shame>>> GetShamesPerLocation(ShameLocation location, MetricType? metricTypeLocation = null) {
		var graveyard = GetGraveyardOrFail();
		if (graveyard.IsFailed) {
			return graveyard.ToResult();
		}

		var shameDict = graveyard.Value.Shames;

		// Remove shames that are not in the specified location
		var filteredShameDict = shameDict.Select(x => new {
			Key = x.Key,
			Value = x.Value.Where(y => y.Location == location && (metricTypeLocation == null || y.MetricLocation == metricTypeLocation))
		});

		// Get shameList that have at least one item
		var shameList = filteredShameDict.Where(x => x.Value.Any())
			.ToDictionary(x => x.Key, x => x.Value.ToList());

		//return
		return Result.Ok(shameList);
	}

	public Result<List<Shame>> GetShamesForUserPerLocation(DiscordUserId userId, ShameLocation location, MetricType? metricTypeLocation = null) {
		var shames = GetShamesForUser(userId);
		
		if (shames.IsFailed) {
			return shames.ToResult();
		}
		
		var filteredShames = shames.Value.Where(s => s.Location == location && (metricTypeLocation == null || s.MetricLocation == metricTypeLocation)).ToList();
		return Result.Ok(filteredShames);
	}

	public Result AddShame(DiscordUserId userId, Shame shame) {
		var graveyard = GetGraveyardOrFail();
		
		if (graveyard.IsFailed) {
			return graveyard.ToResult();
		}
		
		// Add shame to dict if user is id is equal
		if (graveyard.Value.Shames.ContainsKey(userId)) {
			graveyard.Value.Shames[userId].Add(shame);
		} else {
			graveyard.Value.Shames.Add(userId, new List<Shame> {shame});
		}
		
		// Update graveyard
		return Update(graveyard.Value);
	}

	public Result RemoveShame(DiscordUserId userId, Guid shameId) {
		var graveyard = GetGraveyardOrFail();
		
		if (graveyard.IsFailed) {
			return graveyard.ToResult();
		}

		var shameResult = GetShame(graveyard.Value, userId, shameId);
		if(shameResult.IsFailed) {
			return shameResult.ToResult();
		}

		graveyard.Value.Shames[userId].Remove(shameResult.Value);
		return Update(graveyard.Value);
	}

	public Result<Shame> GetShameById(DiscordUserId userId, Guid shameId) {
		var graveyard = GetGraveyardOrFail();
		
		if (graveyard.IsFailed) {
			return graveyard.ToResult();
		}

		return GetShame(graveyard.Value, userId, shameId);
	}

	private Result<Shame> GetShame(Graveyard graveyard, DiscordUserId userId, Guid shameId) {
		if (!graveyard.Shames.ContainsKey(userId)) {
			return Result.Fail("No shames for user");
		}

		// select shame
		var shame = graveyard.Shames[userId].FirstOrDefault(s => s.Id == shameId);

		// remove shame
		if (shame is null) {
			return Result.Fail($"Shame with id {shameId} not found");
		}

		return Result.Ok(shame);
	}

	public Result UpdateShame(DiscordUserId shamedId, Guid shameId, Shame shame) {
		var graveyard = GetGraveyardOrFail();
		
		if (graveyard.IsFailed) {
			return graveyard.ToResult();
		}
		
		if (!graveyard.Value.Shames.ContainsKey(shamedId)) {
			return Result.Fail("No shames for user");
		}

		// select shame
		var originalShame = graveyard.Value.Shames[shamedId].FirstOrDefault(s => s.Id == shameId);
		graveyard.Value.Shames[shamedId].Remove(originalShame);
		graveyard.Value.Shames[shamedId].Add(shame);
		return Update(graveyard.Value);
	}

	private Result<Graveyard> GetGraveyardOrFail() {
		var graveyard = GetSingle();
		if (graveyard.IsFailed) {
			return graveyard;
		}

		if (graveyard.Value is null) {
			return Result.Fail("No graveyard found");
		}

		return Result.Ok(graveyard.Value);
	}
}
