using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Items;
using LiteDB;

namespace DiscordBot.Common.Models.Commands;

public sealed class SelfCountConfirmCommand : IConfirmCommand {
    public SelfCountConfirmCommand() {
        
    }
    
    public SelfCountConfirmCommand(string title, string description, EmbedFieldDto[] fields, Item item, GuildUser requestedBy,
        GuildUser[] splits, string imageUrl) {
        Item = item;
        RequestedBy = requestedBy;
        RequestedOn = DateTimeOffset.Now;
        Title = title;
        Description = description;
        Fields = fields;
        Splits = splits ?? Array.Empty<GuildUser>();
        ImageUrl = imageUrl;
    }

    public Item Item { get; init; }
    public GuildUser[] Splits { get; init; } = Array.Empty<GuildUser>();
    public bool IsSplit => Item != null && Item.Splittable && Splits != null && Splits.Any();
    public GuildUser RequestedBy { get; init; }
    public DateTimeOffset RequestedOn { get; init; }

    public string ImageUrl { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public EmbedFieldDto[] Fields { get; init; } = Array.Empty<EmbedFieldDto>();
    public GuildUser Handler { get; set; }
}
