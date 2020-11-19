using AutoMapper;
using Discord;
using Discord.Addons.Interactive;
using DiscordBotFanatic.Models.ResponseModels;
using DiscordBotFanatic.Transformers.TypeConverters;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Transformers {
    internal static class Configuration {
        public static Mapper GetMapper() {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<string, Embed>().ConvertUsing<StringToEmbedConverter>();
                cfg.CreateMap<EmbedResponse, Embed>().ConvertUsing<EmbedResponseToEmbedConverter>();
                //cfg.CreateMap<Leaderboard, Embed>().ConvertUsing<LeaderboardToEmbedConverter>();
                cfg.CreateMap<CompetitionLeaderboard, EmbedBuilder>().ConvertUsing<CompetitionLeaderboardToEmbedBuilderConverter>();
            });

            return new Mapper(config);
        }
    }
}