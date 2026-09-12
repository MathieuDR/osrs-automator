using DiscordBot.Common.Models.Data.Confirmation;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

internal sealed class ConfirmConfigurationRepository : BaseSingleRecordLiteDbRepository<ConfirmationConfiguration>, IConfirmConfigurationRepository{
    public ConfirmConfigurationRepository(ILogger logger, DatabaseLease lease) : base(logger, lease) { }
    public override string CollectionName => "confirmationConfiguration";
}
