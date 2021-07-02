using System.Collections.Generic;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.Resolvers {
    internal class WOMDeltaToDeltaDictionaryResolver : IValueResolver<WOMDeltaMetric, DeltaMetric, Dictionary<DeltaType, Delta>> {
        public Dictionary<DeltaType, Delta> Resolve(WOMDeltaMetric source, DeltaMetric destination,
            Dictionary<DeltaType, Delta> destMember, ResolutionContext context) {
            destMember = new Dictionary<DeltaType, Delta>()
                {{DeltaType.Rank, context.Mapper.Map<Delta>(source.Rank)}};

            if (source.Experience != null) {
                destMember.Add(DeltaType.Experience, context.Mapper.Map<Delta>(source.Experience));
            }

            if (source.Score != null) {
                destMember.Add(DeltaType.Score, context.Mapper.Map<Delta>(source.Score));
            }

            if (source.Kills != null) {
                destMember.Add(DeltaType.Kills, context.Mapper.Map<Delta>(source.Kills));
            }
            
            if (source.Value != null) {
                destMember.Add(DeltaType.Value, context.Mapper.Map<Delta>(source.Value));
            }
            
            if (source.Ehb != null) {
                destMember.Add(DeltaType.Hours, context.Mapper.Map<Delta>(source.Ehb));
            }
            
            if (source.Ehp != null) {
                destMember.Add(DeltaType.Hours, context.Mapper.Map<Delta>(source.Ehp));
            }

            // Set correct types.
            foreach (KeyValuePair<DeltaType, Delta> kvp in destMember) {
                kvp.Value.DeltaType = kvp.Key;
            }

            return destMember;
        }
    }
}