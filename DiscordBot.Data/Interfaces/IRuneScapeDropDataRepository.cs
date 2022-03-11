using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Drops;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IRuneScapeDropDataRepository : IRecordRepository<RunescapeDropData> {
    Result<bool> HasActiveDrop(EndpointId endpoint);
    Result<RunescapeDropData> GetActive(EndpointId endpoint);
    Result<RunescapeDropData> GetActive(DiscordUserId endpoint);
    Result CloseActive(EndpointId endpoint);
    Result CloseActive(DiscordUserId endpoint);
}
