using AutoMapper;
using Discord;
using DiscordBotFanatic.Models.ResponseModels;
using DiscordBotFanatic.Transformers.TypeConverters;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Configuration {
    internal static class AutoMapperConfiguration {
        public static IServiceCollection ConfigureAutoMapper(this IServiceCollection services) {
            return services.AddSingleton(GetMapper());
        }

        private static Mapper GetMapper() {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<string, Embed>().ConvertUsing<StringToEmbedConverter>();
                cfg.CreateMap<EmbedResponse, Embed>().ConvertUsing<EmbedResponseToEmbedConverter>();
                //cfg.CreateMap<Leaderboard, Embed>().ConvertUsing<LeaderboardToEmbedConverter>();
                cfg.CreateMap<CompetitionLeaderboard, EmbedBuilder>()
                    .ConvertUsing<CompetitionLeaderboardToEmbedBuilderConverter>();

                cfg.CreateMap<Achievement, EmbedBuilder>()
                    .ConvertUsing<AchievementToEmbedBuilderConverter>();
            });

            return new Mapper(config);
        }
    }
}