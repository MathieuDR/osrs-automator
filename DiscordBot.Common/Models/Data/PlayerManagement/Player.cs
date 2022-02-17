using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.PlayerManagement;

public class Player : BaseGuildModel {
    public Player() { }

    public Player(DiscordGuildId guildId, DiscordUserId creatorId) : base(guildId, creatorId) { }

    public DiscordUserId DiscordUserId => CreatedByDiscordId;
    public int WiseOldManDefaultPlayerId { get; set; }
    public string DefaultPlayerUsername { get; set; }
    public string Nickname { get; set; }

    public List<WiseOldManConnector.Models.Output.Player> CoupledOsrsAccounts { get; set; } =
        new();

    public bool EnforceNameTemplate { get; set; }

    public override void IsValid() {
        if (string.IsNullOrEmpty(DefaultPlayerUsername)) {
            ValidationDictionary.Add(nameof(DefaultPlayerUsername), "No default username.");
        }

        if (WiseOldManDefaultPlayerId < 0) {
            ValidationDictionary.Add(nameof(WiseOldManDefaultPlayerId), "Id should be equal higher then 0.");
        }

        base.IsValid();
    }
}
