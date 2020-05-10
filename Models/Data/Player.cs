namespace DiscordBotFanatic.Models.Data {
    public class Player : BaseModel {
        public string DiscordId { get; set; }
        public int WiseOldManDefaultPlayerId { get; set; }
        public string DefaultPlayerUsername { get; set; }

        public override void IsValid() {
            if (string.IsNullOrEmpty(DiscordId)) {
                ValidationDictionary.Add(nameof(DiscordId),$"Discord Id is null or empty.");
            }

            if (!ulong.TryParse(DiscordId, out ulong discordIdUlong)) {
                ValidationDictionary.Add(nameof(DiscordId),$"Discord Id is not an ulong.");
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