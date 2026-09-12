using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Data.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal class GuildHostingStateRepository : BaseRecordLiteDbRepository<GuildHostingState>, IGuildHostingStateRepository {
	public GuildHostingStateRepository(ILogger<GuildHostingStateRepository> logger, DatabaseLease lease) : base(logger, lease) {
		GetCollection().EnsureIndex(x => x.GuildId, true);
	}

	public override string CollectionName => "guildHosting";

	public Result<GuildHostingState?> GetByGuildId(DiscordGuildId guildId) {
		return Result.Ok(GetCollection().Query().Where(x => x.GuildId == guildId).FirstOrDefault());
	}
}
