using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

internal class ClanFundsRepository : BaseSingleRecordLiteDbRepository<ClanFunds>, IClanFundsRepository {
	public ClanFundsRepository(ILogger<ClanFundsRepository> logger, LiteDatabase database) : base(logger, database) { }
	public override string CollectionName => "ClanFunds";
}
