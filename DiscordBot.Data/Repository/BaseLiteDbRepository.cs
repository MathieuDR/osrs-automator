using System;
using System.Collections.Generic;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository {
    public abstract class BaseLiteDbRepository<T> : IRepository<T> where T : BaseModel, new() {
        public BaseLiteDbRepository(ILogger logger, LiteDatabase database) {
            Logger = logger;
            LiteDatabase = database;
        }

        public ILogger Logger { get; }
        public LiteDatabase LiteDatabase { get; }

        public virtual Result<IEnumerable<T>> GetAll() {
            throw new NotImplementedException();
        }

        public virtual Result<T> Get(ObjectId id) {
            throw new NotImplementedException();
        }

        public virtual Result Insert(T toInsert) {
            throw new NotImplementedException();
        }

        public virtual Result Update(T toUpdate) {
            throw new NotImplementedException();
        }

        public virtual Result UpdateOrInsert(T entity) {
            throw new NotImplementedException();
        }

        public virtual Result Delete { get; set; }
    }
}