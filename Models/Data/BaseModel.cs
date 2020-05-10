using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using LiteDB;

namespace DiscordBotFanatic.Models.Data {
    public class BaseModel {
        // ReSharper disable once InconsistentNaming
        public ObjectId _id { get; set; }

        public short Id => _id.Pid;

        protected Dictionary<string, string> ValidationDictionary = new Dictionary<string, string>();

        public virtual void IsValid() {
            if (ValidationDictionary.Any()) {
                throw new ValidationException(ValidationDictionary.Select(x => $"{x.Key} - {x.Value}").Aggregate((i, j) => i + ", " + j));
            }
        }
    }
}