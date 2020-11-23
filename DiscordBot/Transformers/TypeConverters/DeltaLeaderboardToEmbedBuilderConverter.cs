using System;
using System.Text;
using AutoMapper;
using Discord;
using DiscordBotFanatic.Helpers;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Transformers.TypeConverters {
    public class DeltaLeaderboardToEmbedBuilderConverter : ITypeConverter<DeltaLeaderboard, EmbedBuilder> {
        private static readonly int IdHash = 3;
        private static readonly int IdSpace = 5;
        private static readonly int NameSpace = 15;
        private static readonly int NumberSpace = 6;

        public EmbedBuilder Convert(DeltaLeaderboard source, EmbedBuilder destination, ResolutionContext context) {
            destination ??= new EmbedBuilder();

            if (string.IsNullOrEmpty(destination.Title)) {
                destination.Title = $"{source.MetricType.ToString()} - leaderboard.";
            }

            StringBuilder description = new StringBuilder();
            description.Append("#".PadLeft(IdHash).PadRight(IdSpace));
            description.Append("Name".PadRight(NameSpace));
            description.Append("Experience");
            description.Append(Environment.NewLine);


            var bound = Math.Min(source.Members.Count, 20);

            for (int i = 0; i < bound; i++) {
                var member = source.Members[i];

                description.Append($"{i + 1}, ".PadLeft(IdSpace));
                description.Append(member.Player.DisplayName.PadRight(NameSpace));
                description.Append(member.Delta.Gained.FormatNumber().PadLeft(NumberSpace) + Environment.NewLine);
            }

            destination.Description = $"```{description}```";

            destination.AddField("Total on leaderboard", source.PageSize, true);
            destination.AddField("Metric", source.MetricType, true);

            return destination;
        }
    }
}