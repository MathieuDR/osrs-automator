using System.Runtime.Serialization;

namespace DiscordBot.Models.Enums {
    public enum CompetitionType {
        Normal,
        Teams,
        
        [EnumMember(Value = "Teams by country")]
        CountryTeams
    }
}
