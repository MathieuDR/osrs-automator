using Discord;
using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IImageService {
        Image DrawSkillImage(MetricType skill, int level);
    }
}