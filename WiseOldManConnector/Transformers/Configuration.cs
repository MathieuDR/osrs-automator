using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;

namespace WiseOldManConnector.Transformers {
    internal static class Configuration {
        public static Mapper GetMapper() {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<SearchResponse, Player>();
            });

            return new Mapper(config);
        }
    }
}