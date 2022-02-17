using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal class RuneScapeDropDataRepository : BaseRecordLiteDbRepository<RunescapeDropData>, IRuneScapeDropDataRepository {
    public RuneScapeDropDataRepository(ILogger<RuneScapeDropDataRepository> logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => "RunescapeDropRecords";

    public Result<bool> HasActiveDrop(ulong endpoint) {
        return Result.Ok(GetCollection()
            .Count(d => !d.IsHandled && d.UserId == endpoint) > 0);
    }

    public Result<RunescapeDropData> GetActive(ulong endpoint) {
        return Result.Ok(GetCollection()
            .FindOne(d => !d.IsHandled && d.UserId == endpoint));
    }

    public Result CloseActive(ulong endpoint) {
        var item = GetActive(endpoint).Value;
        item = item with { IsHandled = true };
        return Update(item);
    }
}
