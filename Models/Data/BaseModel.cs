using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Discord;
using LiteDB;

namespace DiscordBotFanatic.Models.Data {
    public class BaseModel {
        protected Dictionary<string, string> ValidationDictionary = new Dictionary<string, string>();

        public BaseModel() {
            CreatedOn = DateTime.Now;
        }

        public BaseModel(IUser user) : this(user.Id) { }

        public BaseModel(ulong userId) : this() {
            CreatedByDiscordId = userId;
        }

        // ReSharper disable once InconsistentNaming
        [BsonId]
        public ObjectId _id { get; set; }

        public ulong CreatedByDiscordId { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual void IsValid() {
            if (CreatedByDiscordId <= 0){
                ValidationDictionary.Add(nameof(CreatedByDiscordId), $"created by Id must be higher then 0");
            } 

            if (ValidationDictionary.Any()) {
                throw new ValidationException(ValidationDictionary.Select(x => $"{x.Key} - {x.Value}").Aggregate((i, j) => i + ", " + j));
            }
        }
    }
}