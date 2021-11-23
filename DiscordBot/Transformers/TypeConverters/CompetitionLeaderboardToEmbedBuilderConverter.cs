using System.Text;
using AutoMapper;

namespace DiscordBot.Transformers.TypeConverters;

public class CompetitionLeaderboardToEmbedBuilderConverter : ITypeConverter<CompetitionLeaderboard, EmbedBuilder> {
    private static readonly int IdHash = 3;
    private static readonly int IdSpace = 5;
    private static readonly int NameSpace = 15;
    private static readonly int NumberSpace = 6;

    public EmbedBuilder Convert(CompetitionLeaderboard source, EmbedBuilder destination, ResolutionContext context) {
        destination ??= new EmbedBuilder();

        if (string.IsNullOrEmpty(destination.Title)) {
            destination.Title = $"{source.MetricType.ToString()} competition - leaderboard.";
        }

        var description = new StringBuilder();
        description.Append("#".PadLeft(IdHash).PadRight(IdSpace));
        description.Append("Name".PadRight(NameSpace));
        description.Append("Experience");
        description.Append(Environment.NewLine);


        var bound = Math.Min(source.Members.Count, 20);

        for (var i = 0; i < bound; i++) {
            var competitionParticipant = source.Members[i];

            description.Append($"{(i + 1).ToString()}, ".PadLeft(IdSpace));
            description.Append(competitionParticipant.Player.DisplayName.PadRight(NameSpace));
            description.Append(competitionParticipant.CompetitionDelta.Gained.FormatNumber().PadLeft(NumberSpace) +
                               Environment.NewLine);
        }

        destination.Description = $"```{description}```";

        destination.AddField("Total participants", source.PageSize, true);
        destination.AddField("Metric", source.MetricType, true);

        return destination;
    }
}
