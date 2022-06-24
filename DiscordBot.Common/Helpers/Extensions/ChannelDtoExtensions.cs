using DiscordBot.Common.Dtos.Discord;

namespace DiscordBot.Common.Helpers.Extensions;

public static class ChannelDtoExtensions {

	public static Dictionary<Channel, IEnumerable<Channel>> NestChannels(this IEnumerable<Channel> channels) {
		var enumerable = channels as Channel[] ?? channels.ToArray();
		if (!enumerable.Any(x => x.IsCategoryChannel)) {
			throw new ArgumentException("No category channels found, cannot nest");
		}
		
		var nestedChannels = new Dictionary<Channel, IEnumerable<Channel>>();
		var miscChannels = new List<Channel>();
		
		foreach (var channel in enumerable) {
			if (channel.IsCategoryChannel) {
				var nested = enumerable.Where(x => x.Category == channel.Id).ToArray();
				var textChannels = nested.Where(x => x.IsTextChannel).ToArray();
				var textChannelEnd = textChannels.Any() ? textChannels.Max(x=>x.Order) : 0;
				nested = nested.OrderBy(x => {
					if (x.IsTextChannel) {
						return x.Order;
					}

					return x.Order + textChannelEnd;

				}).ToArray();
				nestedChannels.Add(channel, nested);
			}else if(channel.Category == DiscordChannelId.Empty) {
				// add to misc
				miscChannels.Add(channel);
			}
		}
		
		if (miscChannels.Any()) {
			nestedChannels.Add(new Channel() { Id = DiscordChannelId.Empty, Name = "Uncategorized", Order = -1}, miscChannels);
		}

		return nestedChannels
			.OrderBy(x=>x.Key.Order)
			.ToDictionary(x=>x.Key, x=>x.Value);
	}
	
	public static IEnumerable<Channel> OrderByDiscordPosition(this IEnumerable<Channel> channels) {
		var enumerable = channels.OrderBy(x => x.Order).ToArray();
		
		if (!enumerable.Any(x => x.IsCategoryChannel)) {
			return enumerable;
		}

		var result = new List<Channel>();
		var nested = enumerable.NestChannels();
		foreach (var (key, value) in nested) {
			result.Add(key);
			result.AddRange(value);
		}
		
		return result;
	}
}
