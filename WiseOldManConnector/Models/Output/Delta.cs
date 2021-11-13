using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output; 

public class Delta {
    public DeltaType DeltaType { get; set; }
    public double Start { get; set; }
    public double End { get; set; }
    public double Gained { get; set; }
}