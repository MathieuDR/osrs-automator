using AutoMapper;
using Discord;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Transformers.TypeConverters {
    public class LeaderboardToEmbedConverter : ITypeConverter<Leaderboard, Embed> {
        public Embed Convert(Leaderboard source, Embed destination, ResolutionContext context) {
            throw new System.NotImplementedException();
        }
    }
}