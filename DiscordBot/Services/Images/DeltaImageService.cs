using System;
using DiscordBot.Services.interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Services.Images {
    public class DeltaImageService : ScrollImageServiceBase<Delta> {
        public DeltaImageService(ILogService logService) : base(logService) {
            throw new NotImplementedException();
        }

        protected override Image CreateImage(Delta info) {
            throw new NotImplementedException();
        }

        protected override void MutateScrollWithInfo(IImageProcessingContext context, Delta info) {
            throw new NotImplementedException();
        }

        protected override void MutateScrollWithHeaders(IImageProcessingContext context) {
            throw new NotImplementedException();
        }

        private Color GetFontColor(Delta info, Color normalColor, Color lostRankColor) {
            throw new NotImplementedException();
        }
    }
}