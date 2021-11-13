using System.Linq;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

public class PlayerRepository : BaseLiteDbRepository<Player>, IPlayerRepository {
    public PlayerRepository(ILogger<PlayerRepository> logger, LiteDatabase database) : base(logger, database) { }

    // public Player CoupleOsrsPlayerToGuild(ulong guildId, ulong discordUserId,
    //     WiseOldManConnector.Models.Output.Player womPlayer) {
    //     var player = GetPlayerById(guildId, discordUserId);
    //     if (player == null) {
    //         player = new Player(guildId, discordUserId);
    //         player.CoupledOsrsAccounts.Add(womPlayer);
    //
    //
    //         return InsertPlayerForGuild(guildId, player);
    //     }
    //
    //     player.CoupledOsrsAccounts.Add(womPlayer);
    //     return UpdateOrInsertPlayerForGuild(guildId, player);
    // }

    public override string CollectionName => "players";

    public Result<Player> GetByDiscordId(ulong id) {
        return Result.Ok(GetCollection()
            .FindOne(p => p.DiscordId == id));
    }

    public Result<Player> GetPlayerByOsrsAccount(int womId) {
        return Result.Ok(GetCollection()
            .FindOne(p => p.CoupledOsrsAccounts.Select(wom => wom.Id).Any(id => id == womId)));
    }

    public Result<Player> GetPlayerByOsrsAccount(string username) {
        return Result.Ok(GetCollection()
            .FindOne(p => p.CoupledOsrsAccounts.Select(wom => wom.Username).Any(name => name == username)));
    }
}