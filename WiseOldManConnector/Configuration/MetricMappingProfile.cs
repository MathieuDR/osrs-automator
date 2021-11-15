using AutoMapper;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnector.Transformers.TypeConverters;

namespace WiseOldManConnector.Configuration;

public class MetricMappingProfile : Profile {
    public MetricMappingProfile() {
        CreateMap<string, MetricType>().ConvertUsing<StringToMetricTypeConverter>();
        CreateMap<MetricType, DeltaType>().ConvertUsing<MetricTypeToDeltaTypeConverter>();
    }
}
