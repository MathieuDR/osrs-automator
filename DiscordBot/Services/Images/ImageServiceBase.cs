using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordBot.Helpers;
using DiscordBot.Services.interfaces;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using FontCollection = SixLabors.Fonts.FontCollection;
using Image = SixLabors.ImageSharp.Image;

namespace DiscordBot.Services.Images {
    public abstract class ImageServiceBase<T> : IImageService<T> where T : class {
        protected const string ResourcePath = "Images";

        private readonly FontCollection _collection;
        protected readonly string BasePath;
        protected readonly string BgPath;
        protected readonly TextGraphicsOptions CenterFontOptions;
        protected readonly Color FontColor;
        protected readonly Font HeaderFont;
        protected readonly string IconPath;
        protected readonly Font InfoFont;
        protected readonly TextGraphicsOptions LeftAlignFontOptions;
        protected readonly ILogService LogService;
        protected readonly Color ShadowColor;
        protected readonly Point ShadowOffset;


        public ImageServiceBase(ILogService logService) {
            LogService = logService;
            BasePath = Directory.GetCurrentDirectory();

            IconPath = Path.Join(BasePath, ResourcePath, "icons");
            BgPath = Path.Join(BasePath, ResourcePath, "backgrounds");
            ShadowOffset = new Point(2, 3);

            // Fonts
            _collection = new FontCollection();
            InfoFont = GetRunescapeChatFont(64, FontStyle.Regular);
            HeaderFont = GetRunescapeChatFont(70, FontStyle.Bold);

            FontColor = Color.Black;
            ShadowColor = Color.DimGrey;

            CenterFontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Center,
                }
            };

            LeftAlignFontOptions = new TextGraphicsOptions() {
                TextOptions = new TextOptions() {
                    HorizontalAlignment = HorizontalAlignment.Left
                }
            };
        }

        public Discord.Image GetImage(T info) {
            BeforeGetImage(info);

            Stopwatch t = new Stopwatch();
            t.Start();
            Image image = CreateImage(info);
            Discord.Image discordImage = ImageToDiscordImage(image);
            t.Stop();
            LogService.LogStopWatch(nameof(GetImages), t);

            return discordImage;
        }

        public Discord.Image GetImages(IEnumerable<T> infoEnumerable) {
            var baseInfos = infoEnumerable.ToList();
            BeforeGetImages(baseInfos);

            Stopwatch t = new Stopwatch();
            t.Start();
            Image image = CreateImageForMultipleInfos(baseInfos);
            Discord.Image discordImage = ImageToDiscordImage(image);
            t.Stop();
            LogService.LogStopWatch(nameof(GetImages), t);

            return discordImage;
        }

        protected virtual void BeforeGetImages(IEnumerable<T> infoEnumerable) { }

        protected virtual void BeforeGetImage(T info) { }

        #region image manipulation helpers

        protected Image ConcatImages(List<Image> images, int maxHorizontal, Size gutter, Size margin) {
            return ConcatImages(images, maxHorizontal, gutter.Width, gutter.Height, margin.Width, margin.Height);
        }

        protected Image ConcatImages(List<Image> images, int maxHorizontal, int xGutter, int yGutter, int xMargin, int yMargin) {
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

        protected Image GetIconImage(MetricType type, Size size) {
            Image iconImage = Image.Load(GetIconPathForMetricType(type));
            iconImage.Mutate(x => x.Resize(new ResizeOptions() {
                Mode = ResizeMode.Max,
                Size = size
            }));

            return iconImage;
        }

        #endregion


        #region image creation

        protected abstract Image CreateImage(T info);
        protected abstract Image CreateImageForMultipleInfos(IEnumerable<T> infos);

        #endregion

        #region helpers

        private void InstalledFontCollection(FontCollection collection, string file) {
            var path = Path.Join(BasePath, ResourcePath, "fonts", file);
            collection.Install(path);
        }

        protected Font GetRunescapeChatFont(float size, FontStyle style) {
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

        protected Point CalculatePointFromCenter(Size size, Point center) {
            int x = Math.Max(center.X - (size.Width / 2), 0);
            int y = Math.Max(center.Y - (size.Height / 2), 0);
            return new Point(x, y);
        }

        protected string GetIconPathForMetricType(MetricType skill) {
            var fileName = Regex.Replace(skill.ToString(), @"(?<!_|^)([A-Z])", "_$1").ToLowerInvariant();
            return Path.Join(IconPath, $"{fileName}.png");
        }

        protected string GetBackgroundPathForMetricType(MetricType skill) {
            return Path.Join(BgPath, $"{skill.ToString().ToLowerInvariant()}.png");
        }

        #endregion
    }
}