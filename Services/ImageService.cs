using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using DiscordBotFanatic.Services.interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using FontCollection = SixLabors.Fonts.FontCollection;
using Image = SixLabors.ImageSharp.Image;

// ReSharper disable AccessToDisposedClosure

namespace DiscordBotFanatic.Services {
    public class ImageService : IImageService {
        private readonly string _basePath;
        private readonly string _bgPath;
        private readonly FontCollection _collection;
        private readonly string _iconPath;
        private readonly ILogService _logService;
        private readonly int _maxScrolls = 2;
        private readonly string _scrollBase;

        public ImageService(ILogService logService) {
            _logService = logService;
            _basePath = Directory.GetCurrentDirectory();
            _iconPath = $"{_basePath}\\Images\\icons";
            _bgPath = $"{_basePath}\\Images\\backgrounds";
            _scrollBase = $"{_bgPath}\\scroll\\";

            _collection = new FontCollection();
        }

        public Discord.Image GetImageFromMetric(MetricType type, Metric metric) {
            Stopwatch t = new Stopwatch();
            t.Start();
            Image image = CreateSpecificMetricImage(type, metric);
            Discord.Image discordImage = ImageToDiscordImage(image);
            t.Stop();
            _logService.LogStopWatch(nameof(GetImageFromMetrics), t);

            return discordImage;
        }

        public Discord.Image GetImageFromMetric(Tuple<MetricType, Metric> metricTuple) {
            return GetImageFromMetric(metricTuple.Item1, metricTuple.Item2);
        }

        #region image manipulation helpers

        private Image ConcatImages(List<Image> images, int maxHorizontal, int xGutter, int yGutter, int xMargin, int yMargin) {
            if (!images.Any())
                return null;

            List<Image> horizontalImages = new List<Image>();
            var toConcatImages = images.Chunk(maxHorizontal).Select(x => x.ToList());

            int horizontalMargin = xMargin * 2;
            int maxWidth = horizontalMargin;
            int totalHeight = yMargin * 2;

            foreach (List<Image> concatImage in toConcatImages) {
                var listOfImages = concatImage.ToList();

                int width = listOfImages.Sum(x => x.Width) + (listOfImages.Count - 1) * xGutter + horizontalMargin;
                int height = listOfImages.Max(x => x.Height);

                Image horizontalImage = new Image<Rgba32>(width, height);
                int xOffset = xMargin;
                foreach (Image toAdd in concatImage) {
                    var offset = xOffset;
                    horizontalImage.Mutate(x => { x.DrawImage(toAdd, new Point(offset, 0), new GraphicsOptions()); });
                    xOffset += toAdd.Width + xGutter;

                    toAdd.Dispose();
                }

                horizontalImages.Add(horizontalImage);
                maxWidth = Math.Max(maxWidth, width);
                totalHeight += height + yGutter;
            }

            Image resultImage = new Image<Rgba32>(maxWidth, totalHeight);
            int yOffset = yMargin;
            foreach (Image horizontalImage in horizontalImages) {
                var offset = yOffset;
                resultImage.Mutate(x => { x.DrawImage(horizontalImage, new Point(0, offset), new GraphicsOptions()); });
                yOffset += horizontalImage.Height + yGutter;

                horizontalImage.Dispose();
            }

            return resultImage;
        }

        #endregion
        
        private string GetIconPath(MetricType skill) {
            return $"{_iconPath}\\{skill.ToString().ToLowerInvariant()}.png";
        }

        private string GetSpecificBackground(MetricType skill) {
            return $"{_bgPath}\\{skill.ToString().ToLowerInvariant()}.png";
        }


        #region image creation

        public Discord.Image GetImageFromMetrics(IEnumerable<Tuple<MetricType, Metric>> metricEnumerable) {
            var tuples = metricEnumerable.ToList();
            var lists = new List<List<Tuple<MetricType, Metric>>>();

            if (tuples.Count() > _maxScrolls * 3) {
                int chunkSize = (int) (Math.Ceiling(tuples.Count / (double) _maxScrolls));
                lists = tuples.Chunk(chunkSize).Select(x => x.ToList()).ToList();
            } else {
                lists.Add(tuples);
            }


            Stopwatch t = new Stopwatch();
            t.Start();
            var images = new List<Image>();
            foreach (IEnumerable<Tuple<MetricType, Metric>> list in lists) {
                var image = CreateMetricScroll(list);
                images.Add(image);
            }

            Image resultImage = ConcatImages(images, 3, 300, 50, 150, 0);
            Discord.Image discordImage = ImageToDiscordImage(resultImage);

            // Disposing
            resultImage.Dispose();
            foreach (Image image in images) {
                image.Dispose();
            }

            t.Stop();
            _logService.LogStopWatch(nameof(GetImageFromMetrics), t);

            return discordImage;
        }
        
        private Image CreateMetricScroll(IEnumerable<Tuple<MetricType, Metric>> metricEnumerable) {
            var metricList = metricEnumerable.ToList();
            int metricHeightSection = 80;
            Image resultImage;
            Random random = new Random();

            // Fonts
            var metricFont = GetRunescapeChatFont(64, FontStyle.Regular);
            var headerFont = GetRunescapeChatFont(80, FontStyle.Bold);
            int shadowOffsetY = 5;
            int shadowOffsetX = 3;
            Color fontColor = Color.Black;
            Color shadowColor = Color.DimGrey;

            var metricFontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Center,
                }
            };

            var headerFontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Left
                }
            };

            // Load start and end of the scroll
            using Image topImage = Image.Load($"{_scrollBase}scroll_start.png");
            using Image bottomImage = Image.Load($"{_scrollBase}scroll_end.png");
            Image[] randomBg = new Image[1];
            for (int i = 0; i < randomBg.Length; i++) {
                randomBg[i] = Image.Load($"{_scrollBase}scroll_repeat_{i}.png");
            }

            // Create image
            int totalHeight = topImage.Height + bottomImage.Height + (metricList.Count * metricHeightSection);
            resultImage = new Image<Rgba32>(topImage.Width, totalHeight);

            // Text offsets
            int levelXOffset = 140;
            int expXOffset = levelXOffset + 160;
            int rankXOffset = expXOffset + 180;
            int yOffsetHeaderText = 130;

            // Append scrolls
            resultImage.Mutate(x => {
                x.DrawImage(topImage, new Point(0, 0), PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Src, 1);
                x.DrawImage(bottomImage, new Point(0, totalHeight - bottomImage.Height), PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Src, 1);
            });

            // Metrics
            for (int i = 0; i < metricList.Count; i++) {
                Tuple<MetricType, Metric> tuple = metricList[i];

                // Calculating start
                int yOffset = metricHeightSection * i + topImage.Height;
                int yMiddle = yOffset + 40;
                int yMiddleText = yOffset + 17;

                // Add a random BG
                Image bg = randomBg[random.Next(0, randomBg.Length)];

                // Icon
                using Image iconImage = Image.Load(GetIconPath(tuple.Item1));
                iconImage.Mutate(x => x.Resize(new ResizeOptions() {
                    Mode = ResizeMode.Max,
                    Size = new Size(60, 60)
                }));
                Point iconPlace = CalculatePointFromCenter(iconImage.Size(), new Point(55, yMiddle));


                resultImage.Mutate(x => {
                    x.DrawImage(bg, new Point(0, yOffset), new GraphicsOptions());
                    x.DrawImage(iconImage, iconPlace, new GraphicsOptions());

                    x.DrawText(metricFontOptions, tuple.Item2.ToLevel(), metricFont, shadowColor, new PointF(levelXOffset + shadowOffsetX, yMiddleText + shadowOffsetY));
                    x.DrawText(metricFontOptions, tuple.Item2.ToLevel(), metricFont, fontColor, new PointF(levelXOffset, yMiddleText));

                    x.DrawText(metricFontOptions, tuple.Item2.FormattedExperience(), metricFont, shadowColor, new PointF(expXOffset + shadowOffsetX, yMiddleText + shadowOffsetY));
                    x.DrawText(metricFontOptions, tuple.Item2.FormattedExperience(), metricFont, fontColor, new PointF(expXOffset, yMiddleText));

                    x.DrawText(metricFontOptions, tuple.Item2.FormattedRank(), metricFont, shadowColor, new PointF(rankXOffset + shadowOffsetX, yMiddleText + shadowOffsetY));
                    x.DrawText(metricFontOptions, tuple.Item2.FormattedRank(), metricFont, fontColor, new PointF(rankXOffset, yMiddleText));
                });
            }

            // Add headers
            resultImage.Mutate(x => {
                x.DrawText(headerFontOptions, "Lvl.", headerFont, shadowColor, new PointF(levelXOffset - 60 + shadowOffsetX, yOffsetHeaderText + shadowOffsetY));
                x.DrawText(headerFontOptions, "Lvl.", headerFont, fontColor, new PointF(levelXOffset - 60, yOffsetHeaderText));

                x.DrawText(headerFontOptions, "Exp.", headerFont, shadowColor, new PointF(expXOffset - 80 + shadowOffsetX, yOffsetHeaderText + shadowOffsetY));
                x.DrawText(headerFontOptions, "Exp.", headerFont, fontColor, new PointF(expXOffset - 80, yOffsetHeaderText));

                x.DrawText(headerFontOptions, "Rank.", headerFont, shadowColor, new PointF(rankXOffset - 80 + shadowOffsetX, yOffsetHeaderText + shadowOffsetY));
                x.DrawText(headerFontOptions, "Rank.", headerFont, fontColor, new PointF(rankXOffset - 80, yOffsetHeaderText));
            });


            // Dispose
            foreach (Image image in randomBg) {
                image.Dispose();
            }

            topImage.Dispose();
            bottomImage.Dispose();

            return resultImage;
        }
        
        private Image CreateSpecificMetricImage(MetricType skill, Metric metric) {
            Image resultImage = Image.Load(GetSpecificBackground(skill));

            var font = GetRunescapeChatFont(25, FontStyle.Regular);

            var fontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Left
                }
            };
            int shadowXOffset = 3;
            int shadowYOffset = 5;
            int textXOffset = 30;
            int yOffsetLevel = 10;
            int yLineOffset = 30;
            int yOffsetExperience = yOffsetLevel + yLineOffset;
            int yOffsetRank = yOffsetExperience + yLineOffset;

            resultImage.Mutate(x => {
                x.DrawText(fontOptions, $"Level: {metric.ToLevel()}", font, Color.Black, new PointF(textXOffset + shadowXOffset, yOffsetLevel + shadowYOffset));
                x.DrawText(fontOptions, $"Experience: {metric.FormattedExperience()}", font, Color.Black, new PointF(textXOffset + shadowXOffset, yOffsetExperience + shadowYOffset));
                x.DrawText(fontOptions, $"Rank: {metric.FormattedRank()}", font, Color.Black, new PointF(textXOffset + shadowXOffset, yOffsetRank + shadowYOffset));

                x.DrawText(fontOptions, $"Level: {metric.ToLevel()}", font, Color.Yellow, new PointF(textXOffset, yOffsetLevel));
                x.DrawText(fontOptions, $"Experience: {metric.FormattedExperience()}", font, Color.Yellow, new PointF(textXOffset, yOffsetExperience));
                x.DrawText(fontOptions, $"Rank: {metric.FormattedRank()}", font, Color.Yellow, new PointF(textXOffset, yOffsetRank));
            });

            return resultImage;
        }

        #endregion

        #region helpers

        private void InstalledFontCollection(FontCollection collection, string file) {
            collection.Install($"{_basePath}\\Images\\fonts\\{file}");
        }

        private Font GetRunescapeChatFont(float size, FontStyle style) {
            if (!_collection.TryFind("Runescape Chat", out FontFamily fontFamily)) {
                InstalledFontCollection(_collection, "runescape_chat.ttf");

                if (!_collection.TryFind("Runescape Chat", out fontFamily)) {
                    throw new Exception($"Couldn't get font/install it.");
                }
            }

            Font font = fontFamily.CreateFont(size, style);
            return font;
        }

        private MemoryStream ImageToStream(Image image) {
            MemoryStream resultStream = new MemoryStream();
            image.SaveAsPng(resultStream);
            resultStream.Position = 0;
            return resultStream;
        }

        private Discord.Image ImageToDiscordImage(Image image) {
            var outputStream = ImageToStream(image);
            return new Discord.Image(outputStream);
        }
        
        private Point CalculatePointFromCenter(Size size, Point center) {
            int x = Math.Max(center.X - (size.Width / 2), 0);
            int y = Math.Max(center.Y - (size.Height / 2), 0);
            return new Point(x, y);
        }

        #endregion
    }
}