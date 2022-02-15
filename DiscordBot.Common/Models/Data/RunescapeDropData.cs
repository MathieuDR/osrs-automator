using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Common.Models.Data.Base;
using LiteDB;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Models.Data;

public record RunescapeDropData : BaseRecord {
    public Guid Endpoint { get; init; }
    public IEnumerable<RunescapeDrop> Drops { get; init; } = Array.Empty<RunescapeDrop>();
    public bool IsHandled { get; init; }
    public int TotalValue => Drops.Sum(x => x.TotalValue);
    public int TotalHaValue => Drops.Sum(x => x.TotalHaValue);

    [BsonIgnore]
    public IEnumerable<string> DistinctImages => Drops.Select(x => x.Image).Distinct();
    
    public IEnumerable<ulong> GuildsMessaged { get; set; }

    public IEnumerable<(RunescapeDrop.Player player, IEnumerable<RunescapeDrop> drops, int geValue, int haValue)> PlayerDrops =>
	    Drops
		    .GroupBy(x => x.Recipient)
		    .Select(x =>
			    (x.Key,
				    x.AsEnumerable(),
				    x.Sum(d => d.TotalValue),
				    x.Sum(d => d.TotalHaValue)));
}
