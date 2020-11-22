using System;
using System.Text;
using AutoMapper;
using Discord;
using DiscordBotFanatic.Helpers;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Transformers.TypeConverters {
    public class DeltaLeaderboardToEmbedBuilderConverter : ITypeConverter<DeltaLeaderboard, EmbedBuilder> {

        private static int _idHash = 3;
        private static int _idSpace = 5;
        private static int _nameSpace = 15;
        private static int _numberSpace = 6;

        public EmbedBuilder Convert(DeltaLeaderboard source, EmbedBuilder destination, ResolutionContext context) {
            destination ??= new EmbedBuilder();

            if (string.IsNullOrEmpty(destination.Title)) {
                destination.Title = $"{source.MetricType.ToString()} - leaderboard.";
            }

            StringBuilder description = new StringBuilder();
            description.Append("#".PadLeft(_idHash).PadRight(_idSpace));
            description.Append("Name".PadRight(_nameSpace));
            description.Append("Experience");
            description.Append(Environment.NewLine);


            var bound = Math.Min(source.Members.Count, 20);

            for (int i = 0; i < bound; i++) {
                var member = source.Members[i];

                description.Append($"{i + 1 }, ".PadLeft(_idSpace));
                description.Append(member.Player.DisplayName.PadRight(_nameSpace));
                description.Append(member.Delta.Gained.FormatNumber().PadLeft(_numberSpace) + Environment.NewLine);
            }

            destination.Description =  $"```{description}```";

            destination.AddField("Total on leaderboard", source.PageSize, true);
            destination.AddField("Metric", source.MetricType, true);

            return destination;
        }
    }
}