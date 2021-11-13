using System.Collections.Generic;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using Metric = WiseOldManConnector.Models.Output.Metric;

namespace WiseOldManConnector.Transformers.Resolvers; 

internal class MetricToDictionaryResolver : IValueResolver<WOMSnapshot, Snapshot, Dictionary<MetricType, Metric>> {
    public Dictionary<MetricType, Metric> Resolve(WOMSnapshot source, Snapshot destination, Dictionary<MetricType, Metric> destMember,
        ResolutionContext context) {
        var result = new Dictionary<MetricType, Metric>();
        foreach (var kvp in source.Metrics) {
            result.Add(kvp.Key, context.Mapper.Map<Metric>(kvp.Value));
        }

        return result;
    }
}