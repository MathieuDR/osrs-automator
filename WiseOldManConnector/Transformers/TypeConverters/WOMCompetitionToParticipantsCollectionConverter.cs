using System.Collections.Generic;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class WOMCompetitionToParticipantsCollectionConverter : ITypeConverter<WOMCompetition, IEnumerable<CompetitionParticipant>> {
        public IEnumerable<CompetitionParticipant> Convert(WOMCompetition source, IEnumerable<CompetitionParticipant> destination,
            ResolutionContext context) {
            var result = new List<CompetitionParticipant>();
            var metric = context.Mapper.Map<MetricType>(source.Metric);
            var deltaType = context.Mapper.Map<DeltaType>(metric);

            if (source.Participants != null) {
                foreach (var sourceParticipant in source.Participants) {
                    var destinationParticipant = context.Mapper.Map<CompetitionParticipant>(sourceParticipant);
                    if( destinationParticipant.CompetitionDelta is not null) {
                        destinationParticipant.CompetitionDelta.DeltaType = deltaType;
                    }
                    result.Add(destinationParticipant);
                }
            }

            return result;
        }
    }
}
