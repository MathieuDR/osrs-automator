using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal class RuneScapeDropDataRepository : BaseRecordLiteDbRepository<RunescapeDropData>, IRuneScapeDropDataRepository {
    public RuneScapeDropDataRepository(ILogger<RuneScapeDropDataRepository> logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => "RunescapeDropRecords";

    public Result<bool> HasActiveDrop(EndpointId endpoint) {
        return Result.Ok(GetCollection()
            .Count(d => !d.IsHandled && d.Endpoint == endpoint) > 0);
    }

    public Result<RunescapeDropData> GetActive(EndpointId endpoint) {
        return Result.Ok(GetCollection()
            .FindOne(d => !d.IsHandled && d.Endpoint == endpoint));
    }
    
    public Result<RunescapeDropData> GetActive(DiscordUserId userId) {
        return Result.Ok(GetCollection()
            .FindOne(d => !d.IsHandled && d.UserId == userId));
    }

    public Result CloseActive(EndpointId endpoint) {
        var item = GetActive(endpoint).Value;
        return item == null ? Result.Fail($"No active drop found for endpoint {endpoint}") : Close(item);
    }
    
    public Result CloseActive(DiscordUserId userId) {
        var item = GetActive(userId).Value;
        return item == null ? Result.Fail($"No active drop found for endpoint {userId}") : Close(item);
    }
    
    private Result Close(RunescapeDropData item) {
        item = item with { IsHandled = true };
        return Update(item);
    }
}
