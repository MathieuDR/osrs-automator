using DiscordBot.Common.Models.Data.ClanFunds;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

internal class ClanFundsRepository : BaseSingleRecordLiteDbRepository<ClanFunds>, IClanFundsRepository {
	public ClanFundsRepository(ILogger<ClanFundsRepository> logger, DatabaseLease lease) : base(logger, lease) { }
	public override string CollectionName => "ClanFunds";
}
