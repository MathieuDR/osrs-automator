using System.Threading.Tasks;
using DiscordBot.Common.Models.DiscordDtos;
using FluentResults;

namespace DiscordBot.Services.Services.interfaces {
    public interface IDiscordService {
        Task<Result> SetUsername(GuildUser user, string nickname);
    }
}
