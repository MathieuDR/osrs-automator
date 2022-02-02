using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Repository;
using DiscordBot.Data.Strategies;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

internal abstract class BaseGuildConfigurationService : RepositoryService {
	protected BaseGuildConfigurationService(ILogger logger, IRepositoryStrategy repositoryStrategy) :
		base(logger, repositoryStrategy) { }


	protected Result SaveGuildConfig(GuildConfig guildConfig) {
		var repo = GetRepository<GuildConfigRepository>(guildConfig.GuildId);
		return repo.UpdateOrInsert(guildConfig);
	}

	protected GuildConfig GetGuildConfig(GuildUser guildUser) {
		return GetGuildConfig(guildUser.GuildId);
	}

	protected GuildConfig GetGuildConfig(Guild guild) {
		return GetGuildConfig(guild.Id);
	}

	protected GuildConfig GetGuildConfig(ulong guildId) {
		var repo = GetRepository<GuildConfigRepository>(guildId);
		return repo.GetSingle().Value;
	}
}