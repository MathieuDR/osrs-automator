using Humanizer;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public class Competition : IBaseConnectorOutput {
    private CompetitionLeaderboard _leaderboard;
    public int Id { get; set; }
    public string Title { get; set; }
    public MetricType Metric { get; set; }
    public int Score { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public int? GroupId { get; set; }
    public DateTimeOffset CreateDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public TimeSpan Duration => EndDate - StartDate;

    public string DurationDescription => Duration.Humanize(2);
    public int ParticipantCount { get; set; }
    public double TotalGained { get; set; }
    public List<CompetitionParticipant> Participants { get; set; }

    public CompetitionLeaderboard Leaderboard {
        get { return _leaderboard ??= new CompetitionLeaderboard(Participants, Metric); }
    }
}
