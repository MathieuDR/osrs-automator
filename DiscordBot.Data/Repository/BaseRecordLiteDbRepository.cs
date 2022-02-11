using DiscordBot.Common.Models.Data.Base;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal abstract class BaseRecordLiteDbRepository<T> : IRecordRepository<T> where T : BaseRecord, new() {
	public BaseRecordLiteDbRepository(ILogger logger, LiteDatabase database) {
		Logger = logger;
		LiteDatabase = database;
	}

	public ILogger Logger { get; }
	public LiteDatabase LiteDatabase { get; }
	public abstract string CollectionName { get; }

	public virtual Result<IEnumerable<T>> GetAll() {
		return Result.Ok(GetCollection().FindAll());
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
}