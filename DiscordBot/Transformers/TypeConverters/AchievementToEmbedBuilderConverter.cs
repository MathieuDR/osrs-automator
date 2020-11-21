using System;
using System.Text;
using AutoMapper;
using Discord;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Transformers.TypeConverters {
    public class AchievementToEmbedBuilderConverter : ITypeConverter<Achievement, EmbedBuilder> {

        public EmbedBuilder Convert(Achievement source, EmbedBuilder destination, ResolutionContext context) {
            destination ??= new EmbedBuilder();

            if (string.IsNullOrEmpty(destination.Title)) {
                destination.Title = $"New achievement for {source.Player.DisplayName}!";
            }

            DeltaType typeOfMetric = context.Mapper.Map<DeltaType>(source.Metric);

            string append = "";
            switch (typeOfMetric) {
               
                case DeltaType.Score:
                    switch (source.Metric) {
                        case MetricType.BountyHunterHunter:
                        case MetricType.BountyHunterRogue:
                        case MetricType.LastManStanding:
                            append = "rank";
                           break;
                        case MetricType.ClueScrollsAll:
                        case MetricType.ClueScrollsBeginner:
                        case MetricType.ClueScrollsEasy:
                        case MetricType.ClueScrollsMedium:
                        case MetricType.ClueScrollsHard:
                        case MetricType.ClueScrollsElite:
                        case MetricType.ClueScrollsMaster:
                            append = "completed";
                            break;
                    }
                    break;
                case DeltaType.Experience:
                    if (!source.Title.Contains("99")) {
                        append = "experience";
                    } 
                    break;
            }

            destination.Description = $"{source.Player.DisplayName} just got {source.Title} {append}!";


            return destination;
        }
    }
}