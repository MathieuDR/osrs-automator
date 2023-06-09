using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Items; 

public sealed record GuildItems : BaseGuildRecord {
    public GuildItems() : this(new List<Item>()) { }
    public GuildItems(List<Item> items) {
        Items = items ?? new();
    }
    public GuildItems(DiscordGuildId guildId, DiscordUserId userId, List<Item> items) : base(guildId, userId) {
        Items = items ?? new();
    }
    public List<Item> Items { get; init; }
    
    public void Deconstruct(out List<Item> Items) {
        Items = this.Items;
    }
}
