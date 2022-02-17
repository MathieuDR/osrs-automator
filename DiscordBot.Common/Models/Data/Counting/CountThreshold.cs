namespace DiscordBot.Common.Models.Data.Counting;

public class CountThreshold {
    public DiscordUserId CreatorId { get; set; }
    public string CreatorUsername { get; set; }
    public int Threshold { get; set; }
    public DiscordRoleId? GivenRoleId { get; set; }
    public string Name { get; set; }
}

