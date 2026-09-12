using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class GuildHostingStateRepositoryFactory : BaseLiteDbRepositoryFactory<IGuildHostingStateRepository, GuildHostingStateRepository> {
	public GuildHostingStateRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

	public override bool RequiresGuildId => false;

	public override IGuildHostingStateRepository Create(DiscordGuildId guildId) {
		throw new NotImplementedException();
	}

	public override IGuildHostingStateRepository Create() {
		return new GuildHostingStateRepository(GetLogger(), LiteDbManager.LeaseCommon());
	}
}
