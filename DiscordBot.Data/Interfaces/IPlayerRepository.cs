global using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.PlayerManagement;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IPlayerRepository : IRepository<Player> {
    Result<Player> GetByDiscordId(DiscordUserId id);
    Result<Player> GetPlayerByOsrsAccount(int womId);
    Result<Player> GetPlayerByOsrsAccount(string username);
}
