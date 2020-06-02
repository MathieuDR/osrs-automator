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

        public BaseModel(IUser user) : this() {
            CreatedByDiscordId = user.Id.ToString();
        }

        // ReSharper disable once InconsistentNaming
        [BsonId]
        public ObjectId _id { get; set; }

        public string CreatedByDiscordId { get; set; }
        public DateTime CreatedOn { get; set; }

        public virtual void IsValid() {
            if (string.IsNullOrEmpty(CreatedByDiscordId)) {
                ValidationDictionary.Add(nameof(CreatedByDiscordId), $"Null or empty.");
            }

            if (!ulong.TryParse(CreatedByDiscordId, out _)) {
                ValidationDictionary.Add(nameof(CreatedByDiscordId), $"Not an ulong.");
            }

            if (ValidationDictionary.Any()) {
                throw new ValidationException(ValidationDictionary.Select(x => $"{x.Key} - {x.Value}").Aggregate((i, j) => i + ", " + j));
            }
        }
    }
}