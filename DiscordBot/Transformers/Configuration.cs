using AutoMapper;
using Discord;
using Discord.Addons.Interactive;
using DiscordBotFanatic.Models.ResponseModels;
using DiscordBotFanatic.Transformers.TypeConverters;

namespace DiscordBotFanatic.Transformers {
    internal static class Configuration {
        public static Mapper GetMapper() {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<string, Embed>().ConvertUsing<StringToEmbedConverter>();
                cfg.CreateMap<EmbedResponse, Embed>().ConvertUsing<EmbedResponseToEmbedConverter>();
            });

            return new Mapper(config);
        }
    }
}