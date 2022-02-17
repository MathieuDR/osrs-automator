using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Configuration;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IApplicationCommandInfoRepository : IRecordRepository<ApplicationCommandInfo> {
    Task<Result<ApplicationCommandInfo>> GetByCommandName(string command);
}
