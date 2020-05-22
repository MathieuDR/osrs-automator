using System.Drawing;
using System.IO;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Services.interfaces;
using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Colors;

namespace DiscordBotFanatic.Services {
    public class ImageService : IImageService {
        private readonly string _iconPath;
        private readonly string _skillIconBg;

        public ImageService() {
            var basePath = Directory.GetCurrentDirectory();
            _iconPath = $"{basePath}\\Images\\icons";
            _skillIconBg = $"{basePath}\\Images\\backgrounds\\SkillBackground.png";
        }


        public Discord.Image DrawSkillImage(MetricType skill, int level) {

            using MemoryStream bgStream = new MemoryStream(File.ReadAllBytes(_skillIconBg));
            using MemoryStream iconStream =new MemoryStream(File.ReadAllBytes(GetIconPath(skill)));
            using ImageFactory imageFactory = new ImageFactory();

            Image myImage = new Bitmap(iconStream);
            
            var iconLayer = new ImageLayer()
            {
                Image = myImage, Position = new Point(20,15), Size = new Size(80,80)
            };

            var textlayer = new TextLayer() {
                DropShadow = true,
                FontColor = Color.Yellow,
                FontFamily = new FontFamily("Runescape Chat"),
                FontSize = 60,
                Position = new Point(105,12),
                Text = level.ToString()
            };

            var textlayer2 = new TextLayer() {
                DropShadow = true,
                FontColor = Color.Yellow,
                FontFamily = new FontFamily("Runescape Chat"),
                FontSize = 60,
                Position = new Point(163,55),
                Text = "99"
            };

            MemoryStream resultStream = new MemoryStream();
            imageFactory.Load(bgStream).Overlay(iconLayer).Watermark(textlayer).Watermark(textlayer2).Save(resultStream);

            

           
            return new Discord.Image(resultStream);
        }

        private string GetIconPath(MetricType skill) {
            return $"{_iconPath}\\{skill.ToString().ToLowerInvariant()}.png";
        }
    }
}   