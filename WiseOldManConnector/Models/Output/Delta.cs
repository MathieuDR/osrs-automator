using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class Delta {
        public DeltaType DeltaType { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public long Gained { get; set; }

    }
}