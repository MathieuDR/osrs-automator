using AutoMapper;
using DiscordBot.Services.Helpers;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Transformers.TypeConverters;

public class AchievementToEmbedBuilderConverter : ITypeConverter<Achievement, EmbedBuilder> {
    public EmbedBuilder Convert(Achievement source, EmbedBuilder destination, ResolutionContext context) {
        destination ??= new EmbedBuilder();

        var metricWomIcon = source.Metric.ToFriendlyNameOrDefault();
        var metricWomBackground = source.Metric == MetricType.Combat
            ? MetricType.Strength.ToFriendlyNameOrDefault()
            : metricWomIcon;

        destination
            .AddWiseOldMan()
            .AddWiseldManAuthor(source.Player.DisplayName, source.Player.Url())
            .WithThumbnailUrl($"https://wiseoldman.net/img/runescape/icons/{metricWomIcon}.png")
            .WithImageUrl($"https://wiseoldman.net/img/runescape/backgrounds/{metricWomBackground}.png");


        var typeOfMetric = context.Mapper.Map<DeltaType>(source.Metric);

        var append = "";
        switch (typeOfMetric) {
            case DeltaType.Score:
                switch (source.Metric) {
                    case MetricType.BountyHunterHunter:
                    case MetricType.BountyHunterRogue:
                    case MetricType.LastManStanding:
                        append = " rank";
                        break;
                    case MetricType.ClueScrollsAll:
                    case MetricType.ClueScrollsBeginner:
                    case MetricType.ClueScrollsEasy:
                    case MetricType.ClueScrollsMedium:
                    case MetricType.ClueScrollsHard:
                    case MetricType.ClueScrollsElite:
                    case MetricType.ClueScrollsMaster:
                        append = " completed";
                        break;
                }

                break;
            case DeltaType.Experience:
                if (source.Threshold > 99.ToExperience()) {
                    append = " experience";
                }

                break;
        }


        destination.Description = "";
        destination.Title = $"{source.Player.DisplayName} just got {source.Title.ToLowerInvariant()}{append}.";
        destination.AddField("Achieved at", source.AchievedAt.Value.ToString("dddd, dd/MM/yyyy"), true);
        destination.AddField("Metric", source.Metric, true);

        return destination;
    }
}
