using System;
using System.Collections.Generic;
using Discord;

namespace DiscordBotFanatic.Models.Data {
    public class GuildEvent : BaseModel {
        public ulong GuildId { get; set; }
        public string CreatedByDiscordId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime EndTime { get; set; }
        public string Name { get; set; }
        public List<GuildEventCounter> EventCounters { get; set; }
        public int MinimumPerCounter { get; set; }
        public int MaximumPerCounter { get; set; }

        public override void IsValid() {
            if (string.IsNullOrEmpty(CreatedByDiscordId)) {
                ValidationDictionary.Add(nameof(CreatedByDiscordId), $"Null or empty.");
            }

            if (!ulong.TryParse(CreatedByDiscordId, out _)) {
                ValidationDictionary.Add(nameof(CreatedByDiscordId), $"Not an ulong.");
            }

            if (string.IsNullOrEmpty(Name)) {
                ValidationDictionary.Add(nameof(Name), $"No name.");
            }

            if (MaximumPerCounter < MinimumPerCounter) {
                ValidationDictionary.Add(nameof(MaximumPerCounter), $"Is lower then {nameof(MinimumPerCounter)}.");
            }

            if (MinimumPerCounter < 0) {
                ValidationDictionary.Add(nameof(MinimumPerCounter), $"Is lower then 0.");
            }

            base.IsValid();
        }

        public GuildEvent() { }

        public GuildEvent(IGuildUser user, string name, int minimumPerCounter, int maximumPerCounter) {
            CreatedOn = DateTime.Now;
            Name = name;
            GuildId = user.GuildId;
            CreatedByDiscordId = user.Id.ToString();
            MinimumPerCounter = minimumPerCounter;
            MaximumPerCounter = maximumPerCounter;
        }
    }
}