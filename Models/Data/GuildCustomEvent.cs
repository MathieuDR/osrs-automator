using System;
using System.Collections.Generic;
using Discord;

namespace DiscordBotFanatic.Models.Data {
    public class GuildCustomEvent : BaseGuildModel {
        public string Name { get; set; }
        public List<GuildEventCounter> EventCounters { get; set; }
        public int MinimumPerCounter { get; set; }
        public int MaximumPerCounter { get; set; }
        public DateTime EndTime { get; set; }

        public override void IsValid() {
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

        public GuildCustomEvent() { }

        public GuildCustomEvent(IGuildUser user, string name, int minimumPerCounter, int maximumPerCounter) :base(user) {
            Name = name;
            MinimumPerCounter = minimumPerCounter;
            MaximumPerCounter = maximumPerCounter;
        }
    }
}