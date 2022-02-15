using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class ClanFundsLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IClanFundsRepository, ClanFundsRepository> {
	public ClanFundsLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) :
		base(loggerFactory, liteDbManager) { }

	public override bool RequiresGuildId => true;

	public override IClanFundsRepository Create(ulong guildId) {
		return new ClanFundsRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
	}

	public override IClanFundsRepository Create() {
		throw new NotImplementedException();
	}
}
