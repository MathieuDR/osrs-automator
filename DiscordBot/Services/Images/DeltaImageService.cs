using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Services.interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DiscordBotFanatic.Services.Images {
    public class DeltaImageService : ScrollImageServiceBase<DeltaInfo> {
             private readonly int _gainedXOffset;

        private readonly int _levelXOffset = 140;
        private readonly int _rankXOffset;

        public DeltaImageService(ILogService logService) : base(logService) {
            // Text offsets
            _gainedXOffset = _levelXOffset + 150;
            _rankXOffset = _gainedXOffset + 180;
            MaxScrolls = 2;
        }

        protected override Image CreateImage(DeltaInfo info) {
            Image resultImage = Image.Load(GetBackgroundPathForMetricType(info.Type));

            var font = GetRunescapeChatFont(25, FontStyle.Regular);

            int textXOffset = 30;
            int yOffsetLevel = 10;
            int yLineOffset = 30;

            int yOffsetExperience = yOffsetLevel + yLineOffset;
            int yOffsetRank = yOffsetExperience + yLineOffset;

            Color textColor = Color.Yellow;

            resultImage.Mutate(x => {
                var isSkill = info.Type.IsSkillMetric();
                bool isBoss = !isSkill && info.Type.IsBossMetric();

                if (isSkill) {
                    x.DrawShadowedText(LeftAlignFontOptions, $"Level: {info.Info.LevelGained()}", font, new Point(textXOffset, yOffsetLevel), ShadowOffset, textColor, ShadowColor);
                } else {
                    yOffsetExperience = yOffsetExperience - yOffsetLevel*2;
                    yOffsetRank = yOffsetRank - yOffsetLevel;
                }

                string text = info.Type.IsSkillMetric() ? $"Experience: {info.Info.GainedExperience()}" : isBoss ? $"Kills: {info.Info.Kills.Gained.ToString()}" : $"Score: {info.Info.Score.Gained.ToString()}";

                x.DrawShadowedText(LeftAlignFontOptions, text, font, new Point(textXOffset, yOffsetExperience), ShadowOffset, textColor, ShadowColor);

                textColor = GetFontColor(info, textColor, Color.Red);
                x.DrawShadowedText(LeftAlignFontOptions, $"Rank: {info.Info.GainedRanks()}", font, new Point(textXOffset, yOffsetRank), ShadowOffset, textColor, ShadowColor);
            });

            return resultImage;
        }

        private Color GetFontColor(DeltaInfo info, Color normalColor, Color lostRankColor) {
            if (info.Info.Rank.Gained > 0) {
                // Rank is higher then previous time, so lost on the ladder
                return lostRankColor;
            }

            return normalColor;
        }

        protected override void MutateScrollWithInfo(IImageProcessingContext context, DeltaInfo info) {
            // text
            var isSkill = info.Type.IsSkillMetric();
            bool isBoss = !isSkill && info.Type.IsBossMetric();

            // Icon
            using Image iconImage = GetIconImage(info.Type, new Size(60, 60));
            Point iconPlace = CalculatePointFromCenter(iconImage.Size(), new Point(55, MiddleOfCurrentSectionYOffset));
            context.DrawImage(iconImage, iconPlace, new GraphicsOptions());

            if(isSkill) {
                context.DrawShadowedText(CenterFontOptions, info.Info.LevelGained(), InfoFont, new Point(_levelXOffset, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);
            }

            string text = isSkill? $"{info.Info.GainedExperience()}" : isBoss ? $"{info.Info.Kills.Gained.ToString()}":  $"{info.Info.Score.Gained.ToString()}";
            context.DrawShadowedText(CenterFontOptions, text, InfoFont, new Point(_gainedXOffset, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, FontColor, ShadowColor);

            var rankColor = GetFontColor(info, FontColor, Color.Red);
            context.DrawShadowedText(CenterFontOptions, $"{info.Info.GainedRanks()}", InfoFont, new Point(_rankXOffset, MiddleOfCurrentSectionForTextYOffset), ShadowOffset, rankColor, ShadowColor);
        }

        protected override void MutateScrollWithHeaders(IImageProcessingContext context) {
            // text
            context.DrawShadowedText(LeftAlignFontOptions, "Lvl.", HeaderFont, new Point(_levelXOffset - 60 + HeaderTextOffset.Width, HeaderTextOffset.Height), ShadowOffset, FontColor, ShadowColor);
            context.DrawShadowedText(LeftAlignFontOptions, "Gained", HeaderFont, new Point(_gainedXOffset - 95 + HeaderTextOffset.Width, HeaderTextOffset.Height), ShadowOffset, FontColor, ShadowColor);
            context.DrawShadowedText(LeftAlignFontOptions, "Rank.", HeaderFont, new Point(_rankXOffset - 65 + HeaderTextOffset.Width, HeaderTextOffset.Height), ShadowOffset, FontColor, ShadowColor);
        }
    }
}