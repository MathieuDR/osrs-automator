using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data; 

public class ShameConfiguration : BaseGuildModel {
	public List<ulong> OptedInUsers { get; set; } = new List<ulong>();
	public Dictionary<ulong, List<Shame>> Shames { get; set; }
}