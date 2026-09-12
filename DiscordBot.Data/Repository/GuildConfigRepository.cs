using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal class GuildConfigRepository : BaseLiteDbRepository<GuildConfig>, IGuildConfigRepository {
    public GuildConfigRepository(ILogger<GuildConfigRepository> logger, DatabaseLease lease) : base(logger, lease) { }
    public override string CollectionName => "guildConfig";

    public Result<GuildConfig> GetSingle() {
        return Result.Ok(GetCollection().FindAll().SingleOrDefault());
    }
}