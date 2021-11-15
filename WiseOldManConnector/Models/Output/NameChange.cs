using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public class NameChange {
    public int Id { get; set; }

    public int PlayerId { get; set; }
    public string OldName { get; set; }

    public string NewName { get; set; }

    public NameChangeStatus Status { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
