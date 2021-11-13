using System;
using System.Collections.Generic;
using WiseOldManConnector.Interfaces;

namespace WiseOldManConnector.Models.Output; 

public class Group : IBaseConnectorOutput {
    public int Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public int MemberCount { get; set; }
    public List<Player> Members { get; set; }
    public string ClanChat { get; set; }
    public bool Verified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class VerificationGroup : Group, IVerifiable {
    public string VerificationCode { get; set; }
}