using System.Runtime.Serialization;

namespace DiscordBot.Services.Models.Enums {
    public enum CompetitionType {
        Normal,
        Teams,
        
        [EnumMember(Value = "Teams by country")]
        CountryTeams
    }
}
