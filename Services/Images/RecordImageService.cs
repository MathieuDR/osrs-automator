using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Services.interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.Services.Images {
    public class RecordImageService : ScrollImageServiceBase<RecordInfo> {
        private bool _isSameTimePeriod;
        private Period _period;
        private MetricType _type;

        public RecordImageService(ILogService logService) : base(logService) {
            MaxScrolls = 2;
            HeaderSections = 1;
        }

        protected override Image CreateImageForMultipleInfos(IEnumerable<RecordInfo> infos) {
            var recordInfos = infos.ToList();
            _isSameTimePeriod = recordInfos.IsSamePeriod();
            var firstInfo = recordInfos.FirstOrDefault();
            Debug.Assert(firstInfo != null, nameof(firstInfo) + " != null");
            _period = firstInfo.Info.Period;
            _type = firstInfo.Type;
            return base.CreateImageForMultipleInfos(recordInfos.OrderBy(x=>x.Info.Period));
        }

        protected override Image CreateImage(RecordInfo info) {
            Image resultImage = Image.Load(GetBackgroundPathForMetricType(info.Type));

            var font = GetRunescapeChatFont(25, FontStyle.Regular);

            int textXOffset = 30;
            int yOffsetLevel = 20;
            int yLineOffset = 30;

            int yOffsetRecord = yOffsetLevel + yLineOffset;

            Color textColor = Color.Yellow;

            resultImage.Mutate(x => {
                x.DrawShadowedText(LeftAlignFontOptions, $"Period: {info.Info.Period.ToString()}", font, new Point(textXOffset, yOffsetLevel), ShadowOffset, textColor, ShadowColor);
                x.DrawShadowedText(LeftAlignFontOptions, $"Record: {info.Info.Value.FormatNumber()}", font, new Point(textXOffset, yOffsetRecord), ShadowOffset, textColor, ShadowColor);
            });

            return resultImage;
        }

        protected override void MutateScrollWithInfo(IImageProcessingContext context, RecordInfo info) {
            if (_isSameTimePeriod) {
                // Icon
                using Image iconImage = GetIconImage(info.Type, new Size(60, 60));
                Point iconPlace = CalculatePointFromCenter(iconImage.Size(), new Point(80, MiddleOfCurrentSectionYOffset));
                context.DrawImage(iconImage, iconPlace, new GraphicsOptions());

                // text
                context.DrawShadowedText(LeftAlignFontOptions, info.Info.Value.FormatNumber(), InfoFont, new Point(250, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);
            } else {
                context.DrawShadowedText(LeftAlignFontOptions, info.Info.Period.ToString(), InfoFont, new Point(100, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);
                context.DrawShadowedText(LeftAlignFontOptions, info.Info.Value.FormatNumber(), InfoFont, new Point(350, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);
            }
        }

        protected override void MutateScrollWithHeaders(IImageProcessingContext context) {
            if (_isSameTimePeriod) {
                context.DrawShadowedText(CenterFontOptions, $"Records for a {_period.ToString()} ", HeaderFont, new Point(300, HeaderTextOffset.Height + SectionHeight / 2), ShadowOffset, FontColor, ShadowColor);
            } else {
                using Image iconImage = GetIconImage(_type, new Size(80, 80));
                Point iconPlace = CalculatePointFromCenter(iconImage.Size(), new Point(450, HeaderTextOffset.Height + SectionHeight- 20));
               
                context.DrawShadowedText(LeftAlignFontOptions, "Records for ", HeaderFont, new Point(100, HeaderTextOffset.Height + SectionHeight / 2), ShadowOffset, FontColor, ShadowColor);
                context.DrawImage(iconImage, iconPlace, new GraphicsOptions());
            }
        }
    }
}