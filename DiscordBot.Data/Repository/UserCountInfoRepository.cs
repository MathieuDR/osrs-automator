using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

public class UserCountInfoRepository : BaseLiteDbRepository<UserCountInfo>, IUserCountInfoRepository {
    public UserCountInfoRepository(ILogger<UserCountInfoRepository> logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => "guildUserCounts";

    public Result<UserCountInfo> GetByDiscordUserId(ulong id) {
        return Result.Ok(GetCollection().FindOne(x => x.DiscordId == id));
    }
}
