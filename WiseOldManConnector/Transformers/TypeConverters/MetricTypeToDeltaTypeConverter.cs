using System;
using AutoMapper;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class MetricTypeToDeltaTypeConverter : ITypeConverter<MetricType, DeltaType> {
        public DeltaType Convert(MetricType source, DeltaType destination, ResolutionContext context) {
            var category = source.Category();
            switch (category) {
              case MetricTypeCategory.Skills:
                  return DeltaType.Experience;
                case MetricTypeCategory.Bosses:
                    return DeltaType.Kills;
                case MetricTypeCategory.Activities:
                    return DeltaType.Score;
                case MetricTypeCategory.Time:
                    return DeltaType.Hours;
                case MetricTypeCategory.Others:
                    return DeltaType.Score;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}