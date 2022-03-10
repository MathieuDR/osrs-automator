using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class GraveyardLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IGraveyardRepository, GraveyardRepository> {
	public GraveyardLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) :
		base(loggerFactory, liteDbManager) { }

	public override bool RequiresGuildId => true;

	public override IGraveyardRepository Create(DiscordGuildId guildId) {
		return new GraveyardRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
	}

	public override IGraveyardRepository Create() {
		throw new NotImplementedException();
	}
}
