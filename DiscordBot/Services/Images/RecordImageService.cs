using System;
using System.Collections.Generic;
using DiscordBot.Services.interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Services.Images {
    public class RecordImageService : ScrollImageServiceBase<Record> {
        public RecordImageService(ILogService logService) : base(logService) { }

        protected override Image CreateImageForMultipleInfos(IEnumerable<Record> infos) {
            throw new NotImplementedException();
        }

        protected override Image CreateImage(Record info) {
            throw new NotImplementedException();
        }

        protected override void MutateScrollWithInfo(IImageProcessingContext context, Record info) {
            throw new NotImplementedException();
        }

        protected override void MutateScrollWithHeaders(IImageProcessingContext context) {
            throw new NotImplementedException();
        }
    }
}