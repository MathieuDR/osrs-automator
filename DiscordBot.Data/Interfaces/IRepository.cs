using System.Collections.Generic;
using DiscordBot.Common.Models.Data;
using FluentResults;
using LiteDB;

namespace DiscordBot.Data.Interfaces {
    public interface IRepository { }

    public interface IRepository<T> : IRepository where T: BaseModel, new() {
        public Result<IEnumerable<T>> GetAll();
        public Result<T> Get(ObjectId id);
        public Result Insert(T toInsert);
        public Result Update(T toUpdate);
        public Result UpdateOrInsert(T entity);
        public Result Delete { get; set; }
    }
}
