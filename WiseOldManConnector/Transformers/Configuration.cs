using System.Collections.Generic;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnector.Transformers.Resolvers;
using WiseOldManConnector.Transformers.TypeConverters;


namespace WiseOldManConnector.Transformers {
    internal static class Configuration {
        public static Mapper GetMapper() {
            var config = new MapperConfiguration(cfg => {
                //cfg.CreateMap<string, MetricType>().ConvertUsing<StringToMetricTypeConverter>();
                cfg.CreateMap<PlayerResponse, Player>();
                cfg.CreateMap<WiseOldManConnector.Models.API.Responses.Models.Metric, Metric>();
                cfg.CreateMap<Models.API.Responses.Models.Snapshot, Snapshot>()
                    .ForMember(dest => dest.AllMetrics, opt => opt.MapFrom<MetricToDictionaryResolver>());

                cfg.CreateMap<CompetitionResponse, Competition>()
                    .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartsAt))
                    .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndsAt))
                    .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreatedAt))
                    .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedAt))
                    .ForMember(dest => dest.Participants, opt => opt.MapFrom(src => src.ParticipantCount));

                cfg.CreateMap<SearchResponse, Player>();

                cfg.CreateMap<AssertPlayerTypeResponse, PlayerType>()
                    .ConvertUsing<AssertPlayerTypeResponseToPlayerTypeConverter>();

                cfg.CreateMap<AssertDisplayNameResponse, string>().ConvertUsing<AssertDisplayNameResponseToStringConverter>();
                cfg.CreateMap<string, PlayerType>().ConvertUsing<StringToPlayerTypeConverter>();


                cfg.CreateMap<Models.API.Responses.Models.WOMAchievement, Achievement>()
                    .ForMember(dest => dest.AchievedAt, opt => opt.MapFrom(src => src.CreatedAt))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Type))
                    .ForMember(dest => dest.IsMissing, opt => opt.MapFrom(src => src.Missing));
            });

            return new Mapper(config);
        }
    }
}