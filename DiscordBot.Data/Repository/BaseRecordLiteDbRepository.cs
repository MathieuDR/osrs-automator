using DiscordBot.Common.Models.Data.Base;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal abstract class BaseRecordLiteDbRepository<T> : IRecordRepository<T> where T : BaseRecord, new() {
	private readonly DatabaseLease _lease;

	public BaseRecordLiteDbRepository(ILogger logger, DatabaseLease lease) {
		Logger = logger;
		_lease = lease;
	}

	public ILogger Logger { get; }
	public LiteDatabase LiteDatabase => _lease.Database;
	public abstract string CollectionName { get; }

	public virtual Result<IEnumerable<T>> GetAll() {
		IEnumerable<T> all = GetCollection().FindAll().ToList();
		return Result.Ok(all);
	}

	public virtual Result<T> Get(ObjectId id) {
		return Result.Ok(GetCollection().Query().Where(x => x.Id == id).FirstOrDefault());
	}

	public virtual Result Insert(T toInsert) {
		var collection = GetCollection();
		collection.Insert(toInsert);
		return Result.Ok();
	}

	public virtual Result Update(T toUpdate) {
		var collection = GetCollection();
		collection.Update(toUpdate);
		return Result.Ok();
	}

	public virtual Result UpdateOrInsert(T entity) {
		if (entity.Id is not null) {
			return Update(entity);
		}

		return Insert(entity);
	}

	public virtual Result Delete(T toDelete) {
		var collection = GetCollection();
		return collection.Delete(toDelete.Id) ? Result.Ok() : Result.Fail("Delete failed");
	}

	protected ILiteCollection<T> GetCollection() {
		return LiteDatabase.GetCollection<T>(CollectionName);
	}

	public void Dispose() => _lease.Dispose();
}