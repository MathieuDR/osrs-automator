using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

internal sealed class SelfCountConfigurationRepository : BaseSingleRecordLiteDbRepository<SelfCountConfiguration>, ISelfCountConfigurationRepository {
    public SelfCountConfigurationRepository(ILogger logger, DatabaseLease lease) : base(logger, lease) { }
    public override string CollectionName => "selfCountConfiguration";
}
