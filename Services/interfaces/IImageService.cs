using System;
using System.Collections.Generic;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using Image = Discord.Image;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IImageService<T>  where T : BaseInfo {
        Image GetImage(T info);
        Image GetImages(IEnumerable<T> infoEnumerable);
    }
}