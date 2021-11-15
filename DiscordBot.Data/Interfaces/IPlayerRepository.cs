using DiscordBot.Common.Models.Data;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IPlayerRepository : IRepository<Player> {
    Result<Player> GetByDiscordId(ulong id);
    Result<Player> GetPlayerByOsrsAccount(int womId);
    Result<Player> GetPlayerByOsrsAccount(string username);
}
