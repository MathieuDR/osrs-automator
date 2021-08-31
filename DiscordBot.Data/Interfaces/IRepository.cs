using System.Collections.Generic;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Base;
using FluentResults;
using LiteDB;

namespace DiscordBot.Data.Interfaces {
    public interface IRepository { }

    public interface IRepository<T> : IRepository where T : BaseModel, new() {
        public Result<IEnumerable<T>> GetAll();
        public Result<T> Get(ObjectId id);
        public Result Insert(T toInsert);
        public Result Update(T toUpdate);
        public Result UpdateOrInsert(T entity);
        public Result Delete(T toDelete);
    }

    public interface IRecordRepository<T> : IRepository where T : BaseRecord, new() {
        public Result<IEnumerable<T>> GetAll();
        public Result<T> Get(ObjectId id);
        public Result Insert(T toInsert);
        public Result Update(T toUpdate);
        public Result UpdateOrInsert(T entity);
        public Result Delete(T toDelete);
    }

    public interface ISingleRecordRepository<T> : IRecordRepository<T> where T : BaseRecord, new() {
        public Result<T> GetSingle();
    }
}
