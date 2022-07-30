using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class RunescapeDropperGuildConfigurationRepositoryFactory : 
	BaseLiteDbRepositoryFactory<IRunescapeDropperGuildConfigurationRepository, RunescapeDropperGuildConfigurationRepository> {
	public RunescapeDropperGuildConfigurationRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

	public override bool RequiresGuildId => true;

	public override IRunescapeDropperGuildConfigurationRepository Create(DiscordGuildId guildId) {
		return new RunescapeDropperGuildConfigurationRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
	}

	public override IRunescapeDropperGuildConfigurationRepository Create() => throw new NotImplementedException();
}
