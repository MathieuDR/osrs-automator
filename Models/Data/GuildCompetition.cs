using System;
using System.Collections.Generic;
using Discord;

namespace DiscordBotFanatic.Models.Data {
    public class GuildCompetition :BaseGuildModel {

        public int Id { get; set; }
        public DateTime EndTime { get; set; }

        public Dictionary<string, int> Offsets { get; set; } = new Dictionary<string, int>();

        public bool IsActive {
            get {
                return DateTime.UtcNow >= EndTime;
            }
        }

        public override void IsValid() {
            if (EndTime <= CreatedOn) {
                ValidationDictionary.Add(nameof(EndTime), $"Endtime should be bigger then the creation date.");
            }

            if (Id <= 0) {
                ValidationDictionary.Add(nameof(Id), $"Id should be higher then 0");
            }

            base.IsValid();
        }

        public GuildCompetition() {
            
        }

        public GuildCompetition(IGuildUser user, int id) : base(user) {
            Id = id;
            EndTime = DateTime.MaxValue;
        }
    }
}