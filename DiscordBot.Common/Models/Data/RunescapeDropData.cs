using System;
using System.Collections.Generic;
using System.Linq;
using DiscordBot.Common.Dtos.Runescape;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Models.Data {
    public record RunescapeDropData : BaseRecord {
        public Guid Endpoint { get; init; }
        public IEnumerable<RunescapeDrop> Drops { get; init; } = new RunescapeDrop[0];
        public IEnumerable<string> Images { get; init; } = new string[0];
        public DateTime Created { get; init; }
        public bool IsHandled { get; init; }

        public int TotalValue => Drops.Sum(x => x.TotalValue);
        public int TotalHaValue => Drops.Sum(x => x.TotalHaValue);
        public PlayerType RecipientPlayerType { get; init; }
        public string RecipientUsername { get; init; }
    }
}
