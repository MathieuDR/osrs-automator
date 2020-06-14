using Discord;

namespace DiscordBotFanatic.Models.Data {
    public class Player : BaseModel {
        public Player() { }
        public Player(IUser user) : base(user) { }

        public Player(ulong userId) : base(userId) {
            DiscordId = userId;
        }

        public ulong DiscordId { get; set; }
        public int WiseOldManDefaultPlayerId { get; set; }
        public string DefaultPlayerUsername { get; set; }

        public override void IsValid() {
            if (DiscordId == 0) {
                ValidationDictionary.Add(nameof(DiscordId),$"Discord Id is 0.");
            }

            if (string.IsNullOrEmpty(DefaultPlayerUsername)) {
                ValidationDictionary.Add(nameof(DefaultPlayerUsername), $"No default username.");
            }

            if (WiseOldManDefaultPlayerId < 0) {
                ValidationDictionary.Add(nameof(WiseOldManDefaultPlayerId), $"Id should be equal higher then 0.");
            }
            
            base.IsValid();
        }
    }
}