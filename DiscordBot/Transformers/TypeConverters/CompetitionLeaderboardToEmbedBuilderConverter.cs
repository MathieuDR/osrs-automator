using System;
using System.Linq;
using System.Text;
using AutoMapper;
using Discord;
using DiscordBotFanatic.Helpers;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Transformers.TypeConverters {
    public class CompetitionLeaderboardToEmbedBuilderConverter : ITypeConverter<CompetitionLeaderboard, EmbedBuilder> {

        private static int _idHash = 3;
        private static int _idSpace = 5;
        private static int _nameSpace = 15;
        private static int _numberSpace = 6;

        public EmbedBuilder Convert(CompetitionLeaderboard source, EmbedBuilder destination, ResolutionContext context) {
            destination ??= new EmbedBuilder();

            if (string.IsNullOrEmpty(destination.Title)) {
                destination.Title = $"{source.MetricType.ToString()} competition - leaderboard.";
            }

            StringBuilder description = new StringBuilder();
            description.Append("#".PadLeft(_idHash).PadRight(_idSpace));
            description.Append("Name".PadRight(_nameSpace));
            description.Append("Experience");
            description.Append(Environment.NewLine);


            var bound = Math.Min(source.Members.Count, 20);

            for (int i = 0; i < bound; i++) {
                CompetitionParticipant competitionParticipant = source.Members[i];

                description.Append($"{i + 1 }, ".PadLeft(_idSpace));
                description.Append(competitionParticipant.Player.DisplayName.PadRight(_nameSpace));
                description.Append(competitionParticipant.CompetitionDelta.Gained.FormatNumber().PadLeft(_numberSpace) + Environment.NewLine);
            }

            destination.Description =  $"```{description}```";

            destination.AddField("Total participants", source.PageSize, true);
            destination.AddField("Metric", source.MetricType, true);

            return destination;
        }
    }
}