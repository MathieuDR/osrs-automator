using System.Collections.Generic;
using System.Linq;
using Discord;
using LiteDB;

namespace DiscordBotFanatic.Models.Data {
    public class CountConfig {
        public ulong OutputChannelId { get; set; }
        
        // public for litedb
        public List<CountTreshold> _tresholds { get; set; } = new List<CountTreshold>();
        
        [BsonIgnore]
        public IReadOnlyList<CountTreshold> Tresholds => _tresholds.AsReadOnly();

        public bool AddTreshold(CountTreshold toAdd) {
            _tresholds.Add(toAdd);
            _tresholds = _tresholds.OrderBy(x => x.Treshold).ToList();
            return true;
        }

        public bool RemoveAtIndex(int index) {
            _tresholds.RemoveAt(index);
            return true;
        }
    }

    public class CountTreshold {
        public ulong CreatorId { get; set; }
        public string CreatorUsername { get; set; }
        public int Treshold { get; set; }
        //public IRole GivenRole { get; set; }
        public ulong? GivenRoleId { get; set; }
        public string name { get; set; }
    }
}
