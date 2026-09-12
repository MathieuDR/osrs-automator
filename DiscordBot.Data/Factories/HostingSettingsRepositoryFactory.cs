using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class HostingSettingsRepositoryFactory : BaseLiteDbRepositoryFactory<IHostingSettingsRepository, HostingSettingsRepository> {
	public HostingSettingsRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

	public override bool RequiresGuildId => false;

	public override IHostingSettingsRepository Create(DiscordGuildId guildId) {
		throw new NotImplementedException();
	}

	public override IHostingSettingsRepository Create() {
		return new HostingSettingsRepository(GetLogger(), LiteDbManager.LeaseCommon());
	}
}
