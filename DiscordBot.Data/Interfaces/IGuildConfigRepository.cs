using DiscordBot.Common.Models.Data;
using FluentResults;

namespace DiscordBot.Data.Interfaces; 

public interface IGuildConfigRepository : IRepository<GuildConfig> {
    Result<GuildConfig> GetSingle();
}