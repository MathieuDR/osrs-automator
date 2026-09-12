using DiscordBot.Common.Models.Data.Hosting;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IGuildHostingStateRepository : IRecordRepository<GuildHostingState> {
	Result<GuildHostingState?> GetByGuildId(DiscordGuildId guildId);
}
