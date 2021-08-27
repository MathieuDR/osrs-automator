using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.DiscordDtos;
using FluentResults;

namespace DiscordBot.Services.Interfaces {
    public interface IDiscordService {
        Task<Result> SetUsername(GuildUser user, string nickname);
        Task<Result> PrintRunescapeDataDrop(RunescapeDropData data, ulong guildId, ulong channelId);
    }
}
