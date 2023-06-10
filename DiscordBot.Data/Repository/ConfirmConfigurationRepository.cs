using DiscordBot.Common.Models.Data.Confirmation;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

internal sealed class ConfirmConfigurationRepository : BaseSingleRecordLiteDbRepository<ConfirmationConfiguration>, IConfirmConfigurationRepository{
    public ConfirmConfigurationRepository(ILogger logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => "confirmationConfiguration";
}
