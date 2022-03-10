using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Drops;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IRuneScapeDropDataRepository : IRecordRepository<RunescapeDropData> {
    Result<bool> HasActiveDrop(DiscordUserId endpoint);
    Result<RunescapeDropData> GetActive(DiscordUserId endpoint);
    Result CloseActive(DiscordUserId endpoint);
}
