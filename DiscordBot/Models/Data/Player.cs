﻿using System.Collections.Generic;
using Discord;

namespace DiscordBotFanatic.Models.Data {
    public class Player : BaseGuildModel {
        public Player() { }

        public Player(ulong guildId, ulong discordId): base(guildId, discordId) {
            
        }

        public Player(IGuildUser user) : base(user) {
            
        }

        public ulong DiscordId => CreatedByDiscordId;
        public int WiseOldManDefaultPlayerId { get; set; }
        public string DefaultPlayerUsername { get; set; }

        public List<WiseOldManConnector.Models.Output.Player> CoupledOsrsAccounts { get; set; } = new List<WiseOldManConnector.Models.Output.Player>();

        public override void IsValid() {
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