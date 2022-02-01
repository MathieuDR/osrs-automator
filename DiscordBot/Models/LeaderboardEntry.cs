using System.Text;

namespace DiscordBot.Models;

public class LeaderboardEntry<T> {
	public string Name { get; set; }
	public T Score { get; set; }
	public int Rank { get; set; }
	
	public LeaderboardEntry(string name, T score, int rank) {
		Name = name;
		Score = score;
		Rank = rank;
	}

	public override string ToString() => $"{Rank}. {Name} - {Score}";

	public string ToStringWithPadding(int rankColumnSize = 5, int nameSize = 20, int scoreSize = 8) {
		var sb = new StringBuilder();
		
		sb.Append($"{Rank.ToString()}, ".PadLeft(rankColumnSize));
		sb.Append($"{Name.PadRight(nameSize)}");
		sb.Append($"{Score.ToString().PadLeft(scoreSize)}");
		
		return sb.ToString();
	}
}
