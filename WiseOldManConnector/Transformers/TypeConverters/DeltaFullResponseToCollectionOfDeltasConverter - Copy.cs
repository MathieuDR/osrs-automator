using System.Collections.Generic;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class DeltaFullResponseToCollectionOfDeltasConverter : ITypeConverter<DeltaFullResponse, IEnumerable<Deltas>> {
        public IEnumerable<Deltas> Convert(DeltaFullResponse source, IEnumerable<Deltas> destination, ResolutionContext context) {
            var result = new List<Deltas>();

            if (source.Day != null) {
                var dayDeltas = context.Mapper.Map<Deltas>(source.Day);
                dayDeltas.Period = Period.Day;
                result.Add(dayDeltas);
            }

            if (source.Week != null) {
                var weekDeltas = context.Mapper.Map<Deltas>(source.Week);
                weekDeltas.Period = Period.Week;
                result.Add(weekDeltas);
            }

            if (source.Month != null) {
                var monthDeltas = context.Mapper.Map<Deltas>(source.Month);
                monthDeltas.Period = Period.Month;
                result.Add(monthDeltas);
            }

            if (source.Year != null) {
                var yearDeltas = context.Mapper.Map<Deltas>(source.Year);
                yearDeltas.Period = Period.Year;
                result.Add(yearDeltas);
            }

            destination = result;
            return destination;
        }
    }
}