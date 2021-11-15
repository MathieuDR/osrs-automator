namespace WiseOldManConnector.Models.Output;

public class Snapshots : IBaseConnectorOutput {
    private List<Snapshot> _combined;
    public List<Snapshot> Day { get; set; }
    public List<Snapshot> Week { get; set; }
    public List<Snapshot> Month { get; set; }
    public List<Snapshot> Year { get; set; }

    public List<Snapshot> Combined {
        get {
            if (_combined == null) {
                var combinedList = new List<Snapshot>();
                combinedList.AddRange(Day);
                combinedList.AddRange(Week);
                combinedList.AddRange(Month);
                combinedList.AddRange(Year);
                _combined = combinedList;
            }

            return _combined;
        }
    }
}
