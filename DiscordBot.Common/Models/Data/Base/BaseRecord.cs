using LiteDB;

namespace DiscordBot.Common.Models.Data.Base;

public record BaseRecord {
	[BsonId]
	public ObjectId Id { get; init; }

	public DateTime CreatedOn { get; init; } = DateTime.Now;

	public virtual Dictionary<string, string> ToDictionary() {
		var type = GetType();
		var props = type.GetProperties();
		var dict = new Dictionary<string, string>();

		foreach (var prp in props) {
			var value = prp.GetValue(this, new object[] { });
			var friendlyValue = "not set";

			if (value != null) {
				friendlyValue = value.ToString();
			}

			dict.Add(prp.Name, friendlyValue);
		}

		return dict;
	}
}
