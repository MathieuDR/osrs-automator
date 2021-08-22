using System.Collections.Generic;
using System.Linq;
using LiteDB;

namespace DiscordBot.Common.Models.Data {
    public class CountConfig {
        public ulong OutputChannelId { get; set; }
        
        // public for litedb
        public List<CountThreshold> _tresholds { get; set; } = new List<CountThreshold>();
        
        [BsonIgnore]
        public IReadOnlyList<CountThreshold> Tresholds => _tresholds.AsReadOnly();

        public bool AddTreshold(CountThreshold toAdd) {
            _tresholds.Add(toAdd);
            _tresholds = _tresholds.OrderBy(x => x.Threshold).ToList();
            return true;
        }

        public bool RemoveAtIndex(int index) {
            _tresholds.RemoveAt(index);
            return true;
        }
    }
}
