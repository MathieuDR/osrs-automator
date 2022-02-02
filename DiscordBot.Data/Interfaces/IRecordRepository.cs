using DiscordBot.Common.Models.Data.Base;
using FluentResults;
using LiteDB;

namespace DiscordBot.Data.Interfaces;

public interface IRecordRepository<T> : IRepository where T : BaseRecord, new() {
	public Result<IEnumerable<T>> GetAll();
	public Result<T> Get(ObjectId id);
	public Result Insert(T toInsert);
	public Result Update(T toUpdate);
	public Result UpdateOrInsert(T entity);
	public Result Delete(T toDelete);
}