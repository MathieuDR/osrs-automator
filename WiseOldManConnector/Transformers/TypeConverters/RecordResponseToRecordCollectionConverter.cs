using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters;

internal class RecordResponseToRecordCollectionConverter : ITypeConverter<RecordResponse, IEnumerable<Record>> {
    public IEnumerable<Record> Convert(RecordResponse source, IEnumerable<Record> destination, ResolutionContext context) {
        var result = new List<Record>();

        if (source.Day != null) {
            var dayRecords = context.Mapper.Map<IEnumerable<Record>>(source.Day).ToList();
            foreach (var dayRecord in dayRecords) {
                dayRecord.Period = Period.Day;
            }

            result.AddRange(dayRecords);
        }

        if (source.Week != null) {
            var weekDeltas = context.Mapper.Map<IEnumerable<Record>>(source.Week).ToList();
            foreach (var weekDelta in weekDeltas) {
                weekDelta.Period = Period.Week;
            }

            result.AddRange(weekDeltas);
        }

        if (source.Month != null) {
            var monthDeltas = context.Mapper.Map<IEnumerable<Record>>(source.Month).ToList();
            foreach (var monthDelta in monthDeltas) {
                monthDelta.Period = Period.Month;
            }

            result.AddRange(monthDeltas);
        }

        if (source.Year != null) {
            var yearDeltas = context.Mapper.Map<IEnumerable<Record>>(source.Year).ToList();
            foreach (var yearDelta in yearDeltas) {
                yearDelta.Period = Period.Year;
            }

            result.AddRange(yearDeltas);
        }

        destination = result;
        return destination;
    }
}
