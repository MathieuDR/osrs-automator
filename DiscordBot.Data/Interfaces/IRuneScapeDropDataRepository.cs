using DiscordBot.Common.Models.Data;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IRuneScapeDropDataRepository : IRecordRepository<RunescapeDropData> {
    Result<bool> HasActiveDrop(ulong endpoint);
    Result<RunescapeDropData> GetActive(ulong endpoint);
    Result CloseActive(ulong endpoint);
}
