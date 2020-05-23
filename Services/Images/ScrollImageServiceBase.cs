﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using DiscordBotFanatic.Services.interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DiscordBotFanatic.Services.Images {
    public abstract class ScrollImageServiceBase<T> : ImageServiceBase<T> where T : BaseInfo {
        private readonly string _scrollBase;
        private int _maxScrolls = 1;

        protected readonly int SectionHeight = 80;
        protected readonly Size HeaderTextOffset;

        protected Size ScrollGutter;
        protected Size ScrollMargin;

        protected int CurrentSectionYOffset= 0;
        protected int MiddleOfCurrentSectionYOffset = 0;
        protected int MiddleOfCurrentSectionForTextYOffset = 0;

        protected ScrollImageServiceBase(ILogService logService) : base(logService) {
            _scrollBase = $"{_bgPath}\\scroll\\";
            HeaderTextOffset = new Size(0,130);
            ScrollGutter = new Size(300, 50);
            ScrollMargin = new Size(150, 0);
        }

        public int MaxScrolls {
            get { return _maxScrolls; }
            set { _maxScrolls = value; }
        }

        protected abstract void MutateScrollWithInfo(IImageProcessingContext context, T info);
        protected abstract void MutateScrollWithHeaders(IImageProcessingContext context);


        protected override Image CreateImageForMultipleInfos(IEnumerable<T> infos) {
            var infoList = infos.ToList();
            var lists = new List<List<T>>();

            if (infoList.Count() > _maxScrolls * 3) {
                int chunkSize = (int) (Math.Ceiling(infoList.Count / (double) _maxScrolls));
                lists = infoList.Chunk(chunkSize).Select(x => x.ToList()).ToList();
            } else {
                lists.Add(infoList);
            }


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var images = new List<Image>();
            foreach (List<T> list in lists) {
                var image = CreateScrollImageForMultipleInfos(list);
                images.Add(image);
            }

            Image resultImage = ConcatImages(images, MaxScrolls, ScrollGutter, ScrollMargin);

            // Disposing
            foreach (Image image in images) {
                image.Dispose();
            }

            stopwatch.Stop();
            LogService.LogStopWatch(nameof(CreateImageForMultipleInfos), stopwatch);

            return resultImage;
        }

       protected Image CreateScrollImageForMultipleInfos(IEnumerable<T> infos) {
            var infoList = infos.ToList();
            int infoHeightSection = 80;
            
            Image resultImage;
            Random random = new Random();

            

            // Load start and end of the scroll
            using Image topImage = Image.Load($"{_scrollBase}scroll_start.png");
            using Image bottomImage = Image.Load($"{_scrollBase}scroll_end.png");
            Image[] randomBg = new Image[1];
            for (int i = 0; i < randomBg.Length; i++) {
                randomBg[i] = Image.Load($"{_scrollBase}scroll_repeat_{i}.png");
            }

            // Create image
            int totalHeight = topImage.Height + bottomImage.Height + (infoList.Count * infoHeightSection);
            resultImage = new Image<Rgba32>(topImage.Width, totalHeight);

            // Append scrolls
            resultImage.Mutate(x => {
                x.DrawImage(topImage, new Point(0, 0), PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Src, 1);
                x.DrawImage(bottomImage, new Point(0, totalHeight - bottomImage.Height), PixelColorBlendingMode.Normal, PixelAlphaCompositionMode.Src, 1);
            });

            // Info
            for (int i = 0; i < infoList.Count; i++) {
                T info = infoList.ElementAt(i);

                // Calculating start
                CurrentSectionYOffset = infoHeightSection * i + topImage.Height;
                MiddleOfCurrentSectionYOffset = CurrentSectionYOffset + 40;
                MiddleOfCurrentSectionForTextYOffset = CurrentSectionYOffset + 17;

                // Add a random BG
                Image bg = randomBg[random.Next(0, randomBg.Length)];

                // Mutate
                resultImage.Mutate(x => {
                    x.DrawImage(bg, new Point(0, CurrentSectionYOffset), new GraphicsOptions());
                    MutateScrollWithInfo(x, info);
                });
            }

            // Add headers
            resultImage.Mutate(MutateScrollWithHeaders);

            // Dispose
            foreach (Image image in randomBg) {
                image.Dispose();
            }

            topImage.Dispose();
            bottomImage.Dispose();

            return resultImage;
        }
    }
}