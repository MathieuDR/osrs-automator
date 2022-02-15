namespace DiscordBot.Common.Models.Data;

public class CountThreshold {
    public ulong CreatorId { get; set; }
    public string CreatorUsername { get; set; }
    public int Threshold { get; set; }
    public ulong? GivenRoleId { get; set; }
    public string Name { get; set; }
}

