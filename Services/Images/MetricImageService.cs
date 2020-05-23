using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Services.interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DiscordBotFanatic.Services.Images {
    public class MetricImageService : ScrollImageServiceBase<MetricInfo> {
        private readonly int _expXOffset;

        private readonly int _levelXOffset = 140;
        private readonly int _rankXOffset;

        public MetricImageService(ILogService logService) : base(logService) {
            // Text offsets
            _expXOffset = _levelXOffset + 160;
            _rankXOffset = _expXOffset + 180;
            MaxScrolls = 2;
        }

        protected override Image CreateImage(MetricInfo info) {
            Image resultImage = Image.Load(GetBackgroundPathForMetricType(info.Type));

            var font = GetRunescapeChatFont(25, FontStyle.Regular);

            int textXOffset = 30;
            int yOffsetLevel = 10;
            int yLineOffset = 30;

            int yOffsetExperience = yOffsetLevel + yLineOffset;
            int yOffsetRank = yOffsetExperience + yLineOffset;

            Color textColor = Color.Yellow;

            resultImage.Mutate(x => {
                x.DrawShadowedText(HeaderFontOptions, $"Level: {info.Metric.ToLevel()}", font, new Point(textXOffset, yOffsetLevel), ShadowOffset, textColor, ShadowColor);
                x.DrawShadowedText(HeaderFontOptions, $"Experience: {info.Metric.FormattedExperience()}", font, new Point(textXOffset, yOffsetExperience), ShadowOffset, textColor, ShadowColor);
                x.DrawShadowedText(HeaderFontOptions, $"Rank: {info.Metric.FormattedRank()}", font, new Point(textXOffset, yOffsetRank), ShadowOffset, textColor, ShadowColor);
            });

            return resultImage;
        }

        protected override void MutateScrollWithInfo(IImageProcessingContext context, MetricInfo info) {
            // Icon
            using Image iconImage = GetIconImage(info.Type, new Size(60, 60));
            Point iconPlace = CalculatePointFromCenter(iconImage.Size(), new Point(55, MiddleOfCurrentSectionYOffset));
            context.DrawImage(iconImage, iconPlace, new GraphicsOptions());

            // text
            context.DrawShadowedText(InfoFontOptions, info.Metric.ToLevel(), InfoFont, new Point(_levelXOffset, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);
            context.DrawShadowedText(InfoFontOptions, info.Metric.FormattedExperience(), InfoFont, new Point(_expXOffset, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);
            context.DrawShadowedText(InfoFontOptions, info.Metric.FormattedRank(), InfoFont, new Point(_rankXOffset, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);
        }

        protected override void MutateScrollWithHeaders(IImageProcessingContext context) {
            // text
            context.DrawShadowedText(HeaderFontOptions, "Lvl.", HeaderFont, new Point(_levelXOffset - 60 + HeaderTextOffset.Width, HeaderTextOffset.Height), ShadowOffset, FontColor, ShadowColor);
            context.DrawShadowedText(HeaderFontOptions, "Exp.", HeaderFont, new Point(_expXOffset - 80 + HeaderTextOffset.Width, HeaderTextOffset.Height), ShadowOffset, FontColor, ShadowColor);
            context.DrawShadowedText(HeaderFontOptions, "Rank.", HeaderFont, new Point(_rankXOffset - 80 + HeaderTextOffset.Width, HeaderTextOffset.Height), ShadowOffset, FontColor, ShadowColor);
        }
    }
}