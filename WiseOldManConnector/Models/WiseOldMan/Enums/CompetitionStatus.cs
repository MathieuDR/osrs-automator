using System.Runtime.Serialization;

namespace WiseOldManConnector.Models.WiseOldMan.Enums {
    public enum CompetitionStatus {
        [EnumMember(Value = "upcoming")]
        Upcoming = 1,
        [EnumMember(Value = "ongoing")]
        Ongoing = 2,
        [EnumMember(Value = "finished")]
        Finished = 3
    }
}