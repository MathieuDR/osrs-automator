using System;
using DiscordBot.Common.Models.Data;
using FluentResults;

namespace DiscordBot.Data.Interfaces; 

public interface IRuneScapeDropDataRepository : IRecordRepository<RunescapeDropData> {
    Result<bool> HasActiveDrop(Guid endpoint);
    Result<RunescapeDropData> GetActive(Guid endpoint);
    Result CloseActive(Guid endpoint);
}