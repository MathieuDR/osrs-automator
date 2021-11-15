namespace WiseOldManConnector.Models.Output;

public interface ILeaderboardMember {
    public Player Player { get; }
    public double Value { get; }
}
