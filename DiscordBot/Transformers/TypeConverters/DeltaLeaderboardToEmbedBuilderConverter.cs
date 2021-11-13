using AutoMapper;

namespace DiscordBot.Transformers.TypeConverters; 

public class DeltaLeaderboardToEmbedBuilderConverter : ITypeConverter<DeltaLeaderboard, EmbedBuilder> {
    public EmbedBuilder Convert(DeltaLeaderboard source, EmbedBuilder destination, ResolutionContext context) {
        destination ??= new EmbedBuilder();

        if (string.IsNullOrEmpty(destination.Title)) {
            destination.Title = $"{source.MetricType.ToString()} - leaderboard.";
        }

        destination.Description = $"```{source.MembersToString()}```";

        destination.AddField("Total on leaderboard", source.PageSize, true);
        destination.AddField("Metric", source.MetricType, true);

        return destination;
    }
}