using LiteDB;

namespace DiscordBot.Common.Models.Data.Counting;

public class CountConfig {
	public DiscordChannelId OutputChannelId { get; set; }

	// public for litedb
	[BsonField("Thresholds")]
	public List<CountThreshold> _thresholds { get; set; } = new();

	[BsonIgnore]
	public IReadOnlyList<CountThreshold> Thresholds => _thresholds.AsReadOnly();

	public bool AddThreshold(CountThreshold toAdd) {
		_thresholds.Add(toAdd);
		_thresholds = _thresholds.OrderBy(x => x.Threshold).ToList();
		return true;
	}

	public bool RemoveAtIndex(int index) {
		_thresholds.RemoveAt(index);
		return true;
	}
}
