namespace DiscordBot.Common.Models.Data.Base;

public class BaseGuildModel : BaseModel {
    public BaseGuildModel() { }

    public BaseGuildModel(ulong guildId, ulong discordId) : base(discordId) {
        GuildId = guildId;
    }

    public ulong GuildId { get; set; }
}

public record BaseGuildRecord : BaseRecord {
    public BaseGuildRecord() { }

    public BaseGuildRecord(ulong guildId, ulong discordId) {
        GuildId = guildId;
        CreatedById = discordId;
    }

    public ulong GuildId { get; init; }
    public ulong CreatedById { get; init; }
}
