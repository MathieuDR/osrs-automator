using System;
using DiscordBotFanatic.Services.interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services.Images {
    public class MetricImageService : ScrollImageServiceBase<DeltaMetric> {

        public MetricImageService(ILogService logService) : base(logService) {

        }

        protected override Image CreateImage(DeltaMetric info) {
            throw new NotImplementedException();
        }

        protected override void MutateScrollWithInfo(IImageProcessingContext context, DeltaMetric info) {
            throw new NotImplementedException();
        }

        protected override void MutateScrollWithHeaders(IImageProcessingContext context) {
            throw new NotImplementedException();
        }
    }
}