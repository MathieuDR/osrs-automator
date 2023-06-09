using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

internal sealed class SelfCountConfigurationRepository : BaseSingleRecordLiteDbRepository<SelfCountConfiguration>, ISelfCountConfigurationRepository {
    public SelfCountConfigurationRepository(ILogger logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => "selfCountConfiguration";
}
