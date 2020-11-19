using System;
using AutoMapper;
using Discord;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Transformers.TypeConverters {
    public class LeaderboardToEmbedBuilderConverter : ITypeConverter<Leaderboard, EmbedBuilder> {
        public EmbedBuilder Convert(Leaderboard source, EmbedBuilder destination, ResolutionContext context) {
            //switch (source) {
            //    case CompetitionLeaderboard competitionLeaderboard:
            //        destination = context.Mapper.Map(competitionLeaderboard, destination);
            //        break;
            //    case RecordLeaderboard recordLeaderboard:
            //        destination = context.Mapper.Map<>()
            //    default:
            //        throw new ArgumentException($"Leaderboard type is out of range: {source.GetType().Name}", nameof(source));
            //}

            //return context.Mapper.Map(source);

            return destination ??= new EmbedBuilder().WithDescription("winnieh the pooh bear.");
        }
    }
}