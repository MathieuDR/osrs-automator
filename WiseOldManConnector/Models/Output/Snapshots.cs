using System.Collections.Generic;

namespace WiseOldManConnector.Models.Output {
    public class Snapshots : WiseOldManObject{
        public List<Snapshot> Day { get; set; }
        public List<Snapshot> Week { get; set; }
        public List<Snapshot> Month { get; set; }
        public List<Snapshot> Year { get; set; }
    }
}