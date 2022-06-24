using DiscordBot.Common.Models.Data.Configuration;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IGuildConfigRepository : IRepository<GuildConfig> {
    Result<GuildConfig> GetSingle();
}
