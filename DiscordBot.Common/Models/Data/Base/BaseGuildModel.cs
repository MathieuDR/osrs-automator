namespace DiscordBot.Common.Models.Data.Base;

public class BaseGuildModel : BaseModel {
    public BaseGuildModel() { }

    public BaseGuildModel(DiscordGuildId guildId, DiscordUserId creatorId) : base(creatorId) {
        GuildId = guildId;
    }

    public DiscordGuildId GuildId { get; set; }
}