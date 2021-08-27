using System;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository {
    public class RuneScapeDropDataRepository : BaseRecordLiteDbRepository<RunescapeDropData>, IRuneScapeDropDataRepository {
        public RuneScapeDropDataRepository(ILogger<RuneScapeDropDataRepository> logger, LiteDatabase database) : base(logger, database) { }
        public override string CollectionName => "RunescapeDropRecords";
        public Result<bool> HasActiveDrop(Guid endpoint) {
            return Result.Ok(GetCollection()
                .Count(d=> !d.IsHandled && d.Endpoint == endpoint) > 0);
        }

        public Result<RunescapeDropData> GetActive(Guid endpoint) {
            return Result.Ok(GetCollection()
                    .FindOne(d=> !d.IsHandled && d.Endpoint == endpoint));
        }

        public Result CloseActive(Guid endpoint) {
            var item = GetActive(endpoint).Value;
            item = item with {IsHandled = true};
            return Update(item);
        }
    }
}
