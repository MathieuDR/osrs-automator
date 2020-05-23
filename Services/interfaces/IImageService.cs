using System;
using System.Collections.Generic;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Image = Discord.Image;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IImageService {
        Image GetImageFromMetric(MetricType type, Metric metric);
        Image GetImageFromMetric(Tuple<MetricType, Metric> metricTuple);
        Image GetImageFromMetrics(IEnumerable<Tuple<MetricType, Metric>> metricEnumerable);
    }
}