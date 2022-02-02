namespace WiseOldManConnector.Models.Output;

public abstract class Leaderboard : IBaseConnectorOutput {
    /// <summary>
    ///     Offset
    /// </summary>
    public int Page { get; set; } = 0;

    /// <summary>
    ///     Limit
    /// </summary>
    public int PageSize { get; set; } = 20;
}

public abstract class Leaderboard<T> : Leaderboard where T : ILeaderboardMember {
    protected Leaderboard() { }

    protected Leaderboard(List<T> items) {
        Members = items;
        PageSize = items.Count;
    }

    /// <summary>
    ///     Response
    /// </summary>
    public List<T> Members { get; set; }
}

// Should we use interfaces? probably..