using DiscordBot.Common.Models.Data;
using FluentResults;

namespace DiscordBot.Data.Interfaces; 

public interface IUserCountInfoRepository : IRepository<UserCountInfo> {
    Result<UserCountInfo> GetByDiscordUserId(ulong id);
}