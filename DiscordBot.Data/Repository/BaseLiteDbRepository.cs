using DiscordBot.Common.Models.Data.Base;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal abstract class BaseLiteDbRepository<T> : IRepository<T> where T : BaseModel, new() {
    public BaseLiteDbRepository(ILogger logger, LiteDatabase database) {
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
        return Result.Ok(GetCollection().Query().Where(x => x._id == id).FirstOrDefault());
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
        if (entity._id is not null) {
            return Update(entity);
        }

        return Insert(entity);
    }

    public virtual Result Delete(T toDelete) {
        var collection = GetCollection();
        return collection.Delete(toDelete._id) ? Result.Ok() : Result.Fail("Delete failed");
    }

    public Result BulkInsert(IEnumerable<T> toInsertModels) {
        var collection = GetCollection();
        collection.InsertBulk(toInsertModels);
        return Result.Ok();
    }

    public Result BulkUpdateOrInsert(IEnumerable<T> toUpsertModels) {
        var models = toUpsertModels as T[] ?? toUpsertModels.ToArray();
        var toUpdate = models.Where(x => x._id is not null);
        var toInsert = models.Where(x => x._id is null);

        var updateResult = toUpdate.Select(Update).Merge();
        var insertResult = BulkInsert(toInsert);

        return updateResult.WithReasons(insertResult.Reasons);
    }

    protected ILiteCollection<T> GetCollection() {
        return LiteDatabase.GetCollection<T>(CollectionName);
    }
}