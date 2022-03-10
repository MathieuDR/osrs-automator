using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

internal abstract class BaseGuildConfigurationService : RepositoryService {
	protected BaseGuildConfigurationService(ILogger logger, IRepositoryStrategy repositoryStrategy) :
		base(logger, repositoryStrategy) { }


	protected Result SaveGuildConfig(GuildConfig guildConfig) {
		var repo = GetRepository<IGuildConfigRepository>(guildConfig.GuildId);
		return repo.UpdateOrInsert(guildConfig);
	}

	protected GuildConfig GetGuildConfig(GuildUser guildUser) {
		return GetGuildConfig(guildUser.GuildId);
	}

	protected GuildConfig GetGuildConfig(Guild guild) {
		return GetGuildConfig(guild.Id);
	}

	protected GuildConfig GetGuildConfig(DiscordGuildId guildId) {
		var repo = GetRepository<IGuildConfigRepository>(guildId);
		return repo.GetSingle().Value;
	}
}