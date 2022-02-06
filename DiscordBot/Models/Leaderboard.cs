using System.ComponentModel;
using System.Text;

namespace DiscordBot.Models; 

public class Leaderboard<T> {
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
}