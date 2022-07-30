using DiscordBot.Common.Models.Data.Counting;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IUserCountInfoRepository : IRepository<UserCountInfo> {
    Result<UserCountInfo> GetByDiscordUserId(DiscordUserId id);
}
