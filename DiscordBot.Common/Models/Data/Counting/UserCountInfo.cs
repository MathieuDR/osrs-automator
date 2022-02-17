using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Counting;

public class UserCountInfo : BaseModel {
    public UserCountInfo() { }
    public UserCountInfo(DiscordUserId userId) : base(userId) { }
    public DiscordUserId DiscordId { get; set; }

    public int CurrentCount => CountHistory.Sum(x => x.Additive);

    public List<Count> CountHistory { get; set; } = new();
}
