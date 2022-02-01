using DiscordBot.Common.Models.Data.Base;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface ISingleRecordRepository<T> : IRecordRepository<T> where T : BaseRecord, new() {
	public Result<T> GetSingle();
}