using DiscordBot.Common.Models.Data.Base;
using DiscordBot.Common.Models.Data.Items;

namespace DiscordBot.Common.Models.Data.Counting; 

public sealed record SelfCountConfiguration : BaseGuildRecord {
    public SelfCountConfiguration() : this(new List<Item>()) { }
    public SelfCountConfiguration(List<Item> items) {
        Items = items ?? new();
    }
    public SelfCountConfiguration(DiscordGuildId guildId, DiscordUserId userId, List<Item> items, DiscordChannelId requestChannel) : base(guildId, userId) {
        Items = items ?? new();
        RequestChannel = requestChannel;
    }
    public List<Item> Items { get; init; }
    public DiscordChannelId RequestChannel { get; init; }
    
    public void Deconstruct(out List<Item> Items, out DiscordChannelId RequestChannel) {
        Items = this.Items;
        RequestChannel = this.RequestChannel;
    }
}
