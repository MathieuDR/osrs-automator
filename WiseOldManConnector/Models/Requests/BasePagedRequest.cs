using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WiseOldManConnector.Models.Requests {
    public abstract class BasePagedRequest {
        protected Dictionary<string, string> ValidationDictionary = new();

        public int Limit { get; set; }
        public int Offset { get; set; }

        public virtual void IsValid() {
            if (Limit <= 0) {
                ValidationDictionary.Add(nameof(Limit), "Limit must be at least 1 or higher");
            }

            if (Offset < 0) {
                ValidationDictionary.Add(nameof(Offset), "Offset must be at least 0 or higher");
            }

            if (ValidationDictionary.Any()) {
                throw new ValidationException(ValidationDictionary.Select(x => $"{x.Key} - {x.Value}").Aggregate((i, j) => i + ", " + j));
            }
        }
    }
}
