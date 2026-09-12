using DiscordBot.Common.Models.Data.Base;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal abstract class BaseSingleRecordLiteDbRepository<T> :BaseRecordLiteDbRepository<T>, ISingleRecordRepository<T> where T : BaseRecord, new() {
	protected BaseSingleRecordLiteDbRepository(ILogger logger, DatabaseLease lease) : base(logger, lease) { }
	public Result<T> GetSingle() => Result.Ok(GetAll().Value.FirstOrDefault());
}