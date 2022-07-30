using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal class RunescapeDropperGuildConfigurationRepository : BaseSingleRecordLiteDbRepository<DropperGuildConfiguration>,
	IRunescapeDropperGuildConfigurationRepository {
	public RunescapeDropperGuildConfigurationRepository(ILogger<RunescapeDropperGuildConfigurationRepository> logger, LiteDatabase database) : base(logger, database) { }
	public override string CollectionName => "runescapeDropperGuildConfiguration";
}
