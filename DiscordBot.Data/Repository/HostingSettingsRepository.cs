using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal class HostingSettingsRepository : BaseSingleRecordLiteDbRepository<HostingSettings>, IHostingSettingsRepository {
	public HostingSettingsRepository(ILogger<HostingSettingsRepository> logger, DatabaseLease lease) : base(logger, lease) { }

	public override string CollectionName => "hostingSettings";
}
