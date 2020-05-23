using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Discord;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using DiscordBotFanatic.Services.interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using FontCollection = SixLabors.Fonts.FontCollection;
using Image = SixLabors.ImageSharp.Image;

namespace DiscordBotFanatic.Services {
    public class ImageService : IImageService {
        private readonly string _basePath;
        private readonly string _iconPath;
        private readonly ILogService _logService;
        private readonly string _skillBg;
        private readonly string _scrollBase;
        private FontCollection _collection;

        public ImageService(ILogService logService) {
            _logService = logService;
            _basePath = Directory.GetCurrentDirectory();
            _iconPath = $"{_basePath}\\Images\\icons";
            _skillBg = $"{_basePath}\\Images\\backgrounds\\skill_bg_3.png";
            _scrollBase = $"{_basePath}\\Images\\backgrounds\\scroll\\";

            _collection = new FontCollection();
        }

        public Discord.Image GetImageFromMetric(MetricType skill, string level, string rank, string exp) {
            var resultImage = CreateSkillImage(skill, level, rank, exp);
            var resultStream = ImageToStream(resultImage);
            return new Discord.Image(resultStream);
        }

        public Discord.Image GetImageFromMetric(MetricType type, Metric metric) {
            return GetImageFromMetric(type, metric.ToLevel(), metric.Rank.FormatNumber(), metric.Experience.FormatNumber());
        }

        public Discord.Image GetImageFromMetric(Tuple<MetricType, Metric> metricTuple) {
            return GetImageFromMetric(metricTuple.Item1, metricTuple.Item2);
        }

        public Discord.Image GetImageFromMetrics(IEnumerable<Tuple<MetricType, Metric>> metricEnumerable) {
            Stopwatch t = new Stopwatch();
            t.Start();
            var result = CreateMetricScroll(metricEnumerable);
            t.Stop();
            _logService.LogStopWatch(nameof(GetImageFromMetrics), t);

            return result;
        }

        private Discord.Image CreateMetricScroll(IEnumerable<Tuple<MetricType, Metric>> metricEnumerable) {
            var metricList = metricEnumerable.ToList();
            int metricHeightSection = 80;
            Image resultImage = null;
            Random random = new Random();

            // Fonts
            var metricFont = GetRunescapeChatFont(64, FontStyle.Regular);
            var headerFont = GetRunescapeChatFont(80, FontStyle.Bold);
            int shadowOffsetY = 5;
            int shadowOffsetX = 3;

            var  metricFontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Center,
                }
            };

            var  headerFontOptions = new TextGraphicsOptions() {
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
                x.DrawImage(bottomImage, new Point(0, totalHeight-bottomImage.Height), PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Src, 1);
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
                iconImage.Mutate(x=>x.Resize(new ResizeOptions() {
                    Mode = ResizeMode.Max,
                    Size = new Size(40,40)
                }));
                Point iconPlace = CalculatePointFromCenter(iconImage.Size(), new Point(60, yMiddle));

                
                resultImage.Mutate(x => {
                    x.DrawImage(bg, new Point(0, yOffset), new GraphicsOptions(){});
                    x.DrawImage(iconImage, iconPlace,new GraphicsOptions(){});
                   
                    x.DrawText(metricFontOptions, tuple.Item2.ToLevel(), metricFont, Color.Black, new PointF(levelXOffset + shadowOffsetX, yMiddleText+shadowOffsetY));
                    x.DrawText(metricFontOptions, tuple.Item2.ToLevel(), metricFont, Color.Yellow, new PointF(levelXOffset, yMiddleText));

                    x.DrawText(metricFontOptions, tuple.Item2.FormattedExperience(), metricFont, Color.Black, new PointF(expXOffset + shadowOffsetX, yMiddleText+shadowOffsetY));
                    x.DrawText(metricFontOptions, tuple.Item2.FormattedExperience(), metricFont, Color.Yellow, new PointF(expXOffset, yMiddleText));

                    x.DrawText(metricFontOptions, tuple.Item2.FormattedRank(), metricFont, Color.Black, new PointF(rankXOffset + shadowOffsetX, yMiddleText+shadowOffsetY));
                    x.DrawText(metricFontOptions, tuple.Item2.FormattedRank(), metricFont, Color.Yellow, new PointF(rankXOffset, yMiddleText));
                });

            }

            // Add headers
            resultImage.Mutate(x => {
                x.DrawText(headerFontOptions, "Lvl.", headerFont, Color.Black, new PointF(levelXOffset - 60+ shadowOffsetX, yOffsetHeaderText+shadowOffsetY));
                x.DrawText(headerFontOptions, "Lvl.", headerFont, Color.Yellow, new PointF(levelXOffset- 60, yOffsetHeaderText));

                x.DrawText(headerFontOptions, "Exp.", headerFont, Color.Black, new PointF(expXOffset - 80 + shadowOffsetX, yOffsetHeaderText+shadowOffsetY));
                x.DrawText(headerFontOptions, "Exp.", headerFont, Color.Yellow, new PointF(expXOffset- 80, yOffsetHeaderText));

                x.DrawText(headerFontOptions, "Rank.", headerFont, Color.Black, new PointF(rankXOffset- 80 + shadowOffsetX, yOffsetHeaderText+shadowOffsetY));
                x.DrawText(headerFontOptions, "Rank.", headerFont, Color.Yellow, new PointF(rankXOffset- 80, yOffsetHeaderText));
            });

            // Save
            MemoryStream output = ImageToStream(resultImage);

            // Dispose
            foreach (Image image in randomBg) {
                image.Dispose();
            }
            topImage.Dispose();
            bottomImage.Dispose();

            return new Discord.Image(output);
        }

        private Discord.Image CreateSkillSquares(IEnumerable<Tuple<MetricType, Metric>> metricEnumerable) {
           
            var metricList = metricEnumerable.ToList();

            Image resultImage = null;
            int maxImagesHorizontal = 3;
            using Image bgImage = Image.Load(_skillBg);
            //using Image bgImage = new Image<Rgba32>(200, 100);


            //var options = new ShapeGraphicsOptions() { };

            //IBrush brush = Brushes.Solid(Color.SaddleBrown);
            //IPath path = new RectangularPolygon(0, 0, 200, 100);

            //bgImage.Mutate(x => x.Fill(brush, path));
            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.MidnightBlue, 6), new PointF[] {new PointF(0, 0), new PointF(200, 0)}));
            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.MidnightBlue, 6), new PointF[] {new PointF(0, 100), new PointF(200, 100)}));
            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.MidnightBlue, 6), new PointF[] {new PointF(0, 0), new PointF(0, 100)}));
            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.MidnightBlue, 6), new PointF[] {new PointF(200, 0), new PointF(200, 100)}));

            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.DimGray, 2), new PointF[] {new PointF(3, 3), new PointF(197, 3)}));
            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.DimGray, 2), new PointF[] {new PointF(3, 97), new PointF(197, 97)}));
            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.DimGray, 2), new PointF[] {new PointF(3, 3), new PointF(3, 97)}));
            //bgImage.Mutate(x => x.DrawLines(Pens.Solid(Color.DimGray, 2), new PointF[] {new PointF(197, 3), new PointF(197, 97)}));

            // Info
            int width = Math.Min(bgImage.Size().Width * maxImagesHorizontal, bgImage.Size().Width * metricList.Count);
            int height = Math.Max(bgImage.Size().Height, bgImage.Size().Height * (int) Math.Ceiling(metricList.Count / (double) maxImagesHorizontal));
            resultImage = new Image<Rgba32>(width, height);
            var normalDrawing = new GraphicsOptions() {ColorBlendingMode = PixelColorBlendingMode.Normal};

            // Fonts
            var bigFont = GetRunescapeChatFont(45, FontStyle.Regular);
            var smallFont = GetRunescapeChatFont(20, FontStyle.Regular);

            var fontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            for (var i = 0; i < metricList.Count; i++) {
                Tuple<MetricType, Metric> tuple = metricList[i];

                // Calculating start
                int xCoord = bgImage.Size().Width * (i % maxImagesHorizontal);
                int yCoord = bgImage.Size().Height * (i / maxImagesHorizontal);

                // Adding BG
                resultImage.Mutate(x => x.DrawImage(bgImage, new Point(xCoord, yCoord), normalDrawing));

                // Adding Icon
                using Image iconImage = Image.Load(GetIconPath(tuple.Item1));
                iconImage.Mutate(x => x.Resize(new ResizeOptions() {
                    Size = new Size(61, 55),
                    Mode = ResizeMode.Max
                }));
                var iconPoint = CalculatePointFromCenter(iconImage.Size(), new Point(40 + xCoord, 35 + yCoord));
                resultImage.Mutate(x => { x.DrawImage(iconImage, iconPoint, new GraphicsOptions() {ColorBlendingMode = PixelColorBlendingMode.Normal}); });

                // Adding scores
                resultImage.Mutate(x => x.DrawText(fontOptions, tuple.Item2.ToLevel(), bigFont, Color.Yellow, new PointF(40 + xCoord, 80 + yCoord)));
                resultImage.Mutate(x => x.DrawText(fontOptions, tuple.Item2.FormattedExperience(), bigFont, Color.Yellow, new PointF(150 + xCoord, 80 + yCoord)));
                resultImage.Mutate(x => x.DrawText(fontOptions, tuple.Item2.FormattedRank(), bigFont, Color.Yellow, new PointF(150 + xCoord, 25 + yCoord)));

                resultImage.Mutate(x => x.DrawText(new TextGraphicsOptions(), "Lvl.", smallFont, Color.Yellow, new PointF(13 + xCoord, 65 + yCoord)));
                resultImage.Mutate(x => x.DrawText(new TextGraphicsOptions(), "Exp.", smallFont, Color.Yellow, new PointF(100 + xCoord, 65 + yCoord)));
                resultImage.Mutate(x => x.DrawText(new TextGraphicsOptions(), "Rank.", smallFont, Color.Yellow, new PointF(100 + xCoord, 10 + yCoord)));
            }

            var outputStream = new MemoryStream();
            resultImage.SaveAsPng(outputStream);
            outputStream.Position = 0;
            return new Discord.Image(outputStream);
        }

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

        private Image CreateSkillImage(MetricType skill, string level, string rank, string exp) {
            Image resultImage = Image.Load(_skillBg);
            using Image iconImage = Image.Load(GetIconPath(skill));

            iconImage.Mutate(x => x.Resize(new ResizeOptions() {
                Size = new Size(61, 55),
                Mode = ResizeMode.Max
            }));

            var iconPoint = CalculatePointFromCenter(iconImage.Size(), new Point(40, 35));

            resultImage.Mutate(x => {
                Debug.Assert(iconImage != null, nameof(iconImage) + " != null");
                x.DrawImage(iconImage, iconPoint, new GraphicsOptions() {ColorBlendingMode = PixelColorBlendingMode.Normal});
            });

            var bigFont = GetRunescapeChatFont(45, FontStyle.Regular);
            var smallFont = GetRunescapeChatFont(20, FontStyle.Regular);

            var fontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            };

            resultImage.Mutate(x => x.DrawText(fontOptions, level, bigFont, Color.Yellow, new PointF(40, 80)));
            resultImage.Mutate(x => x.DrawText(fontOptions, exp, bigFont, Color.Yellow, new PointF(150, 80)));
            resultImage.Mutate(x => x.DrawText(fontOptions, rank, bigFont, Color.Yellow, new PointF(150, 25)));

            resultImage.Mutate(x => x.DrawText(new TextGraphicsOptions(), "Lvl.", smallFont, Color.Yellow, new PointF(13, 62)));
            resultImage.Mutate(x => x.DrawText(new TextGraphicsOptions(), "Exp.", smallFont, Color.Yellow, new PointF(100, 62)));
            resultImage.Mutate(x => x.DrawText(new TextGraphicsOptions(), "Rank.", smallFont, Color.Yellow, new PointF(100, 12)));

            return resultImage;
        }

        private Point CalculatePointFromCenter(Size size, Point center) {
            int x = Math.Max(center.X - (size.Width / 2), 0);
            int y = Math.Max(center.Y - (size.Height / 2), 0);
            return new Point(x, y);
        }

        //private Size CalculateSizeByResolution(Size currentSize, Size newSize) {
        //    //double resolution = currentSize.Width / (double)currentSize.Height;

        //    double widthFactor = newSize.Width / (double) currentSize.Width;
        //    double heightFactor = newSize.Height / (double) currentSize.Height;

        //    double factor = Math.Min(widthFactor, heightFactor);

        //    return new Size((int) (currentSize.Width * factor), (int) (currentSize.Height * factor));
        //}

        private string GetIconPath(MetricType skill) {
            return $"{_iconPath}\\{skill.ToString().ToLowerInvariant()}.png";
        }
    }
}