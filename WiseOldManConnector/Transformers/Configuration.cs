using System.Collections.Generic;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnector.Transformers.Resolvers;


namespace WiseOldManConnector.Transformers {
    internal static class Configuration {
        public static Mapper GetMapper() {
            var config = new MapperConfiguration(cfg => {
                cfg.CreateMap<PlayerResponse, Player>();
                cfg.CreateMap<WiseOldManConnector.Models.API.Responses.Models.Metric, Metric>();
                cfg.CreateMap<Models.API.Responses.Models.Snapshot, Snapshot>()
                    .ForMember(dest => dest.AllMetrics, opt => opt.MapFrom<MetricToDictionaryResolver>());
            });

            return new Mapper(config);
        }
    }
}