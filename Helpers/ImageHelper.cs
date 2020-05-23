using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace DiscordBotFanatic.Helpers {
    public static class ImageHelper {
        public static void DrawShadowedText(this IImageProcessingContext context, TextGraphicsOptions fontOptions ,string text, Font font, Point point, Point shadowOffset, Color textColor , Color shadowColor) {
            context.DrawText(fontOptions, text, font, shadowColor, new PointF(point.X + shadowOffset.X, point.Y + shadowOffset.Y));
            context.DrawText(fontOptions, text, font, textColor, point);
        }
    }
}