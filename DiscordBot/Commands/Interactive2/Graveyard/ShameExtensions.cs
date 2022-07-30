using DiscordBot.Common.Models.Data.Graveyard;
using DiscordBot.Common.Models.Enums;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard; 

public static class ShameExtensions {
	public static (ShameLocation shameLocation, MetricType? shameLocationMetric) ToLocation(string input, MetricTypeParser parser) {
		if (parser.TryParseToMetricType(input, out var metric)) {
			return (ShameLocation.MetricType, metric);
		}
		
		return ((ShameLocation)Enum.Parse(typeof(ShameLocation), input, true), null);
	}

	private static EmbedBuilder AddShameToEmbedBuilder(this EmbedBuilder builder, Shame shame, int? index = null, IUser user = null) {
		// add title
		builder.WithTitle($"Oh the shame...");
		
		// add picture if not null
		if (shame.ImageUrl != null) {
			builder.WithImageUrl(shame.ImageUrl);
		}
		
		// add index if not null
		if (index != null) {
			builder.AddField("Shame #", index.ToString());
		}
		
		// add user if not null
		if (user != null) {
			builder.WithAuthor(user);
		}
		
		// add location field
		builder.AddField("Location", shame.ShameLocationAsString, true);
		
		//add field from who shamed the user
		builder.AddField("Shamed by", shame.ShamedBy.ToUser(), true);
		
		//add timestamp
		builder.AddField("Timestamp", shame.ShamedAt.ToString("dd/MM/yyyy HH:mm:ss"), true);
		
		//add id
		builder.AddField("Id", shame.Id.ToString());

		return builder;
	}

	public static EmbedBuilder WithShame(this EmbedBuilder builder, Shame shame) => builder.AddShameToEmbedBuilder(shame, null);
	public static EmbedBuilder WithShame(this EmbedBuilder builder, Shame shame, IUser user) => builder.AddShameToEmbedBuilder(shame, null, user);
	public static EmbedBuilder WithShame(this EmbedBuilder builder, Shame shame, int index) => builder.AddShameToEmbedBuilder(shame, index);
	public static EmbedBuilder WithShame(this EmbedBuilder builder, Shame shame, int index, IUser user) => builder.AddShameToEmbedBuilder(shame, index, user);
}
