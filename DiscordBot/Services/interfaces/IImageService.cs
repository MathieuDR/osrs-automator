using System;
using System.Collections.Generic;
using Image = Discord.Image;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IImageService<T>  where T : class {
        Image GetImage(T info);
        Image GetImages(IEnumerable<T> infoEnumerable);
    }
}