using System.Text;

namespace DiscordBot.Models; 

public class DiscordLeaderBoard<T> {
	public string Name { get; set; }
	public string Description { get; set; }
	public string ScoreFieldName { get; set; }
	public List<LeaderboardEntry<T>> Entries { get; set; } = new List<LeaderboardEntry<T>>();

	public string ToPaddedHeaderString(int rankColumnSize = 5, int nameSize = 25, int scoreSize = 8) {
		var sb = new StringBuilder();
		
		sb.Append("#, ".PadLeft(rankColumnSize));
		sb.Append("Name".PadRight(nameSize));
		sb.Append(ScoreFieldName.PadLeft(scoreSize));
		
		return sb.ToString();
	}

	public string ToMessage(int maxCharacters = 2000) {
		var sb = new StringBuilder();

		sb.AppendLine($"**{Name}**");
		sb.AppendLine(Description);

		sb.AppendLine("```");
		
		// loop through entries and add to sb
			foreach (var entry in Entries) {
				var entryString = entry.ToStringWithPadding();
				// break if we have exceeded max characters
				if (sb.Length + entryString.Length + 3 >= maxCharacters) {
					break;
				}
			sb.AppendLine(entry.ToString());
		}
		
		sb.Append("```");
		
		// throw exception if we have exceeded max characters
		if (sb.Length >= maxCharacters) {
			throw new Exception($"Leaderboard exceeds max characters of {maxCharacters}");
		}
		
		return sb.ToString();
	}
}