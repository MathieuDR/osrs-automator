using DiscordBot.Common.Models.Commands;
using DiscordBot.Common.Models.Data.Base;
using DiscordBot.Common.Models.Enums;
using LiteDB;

namespace DiscordBot.Common.Models.Data.Confirmation; 

public record Confirmation : BaseRecord {
    public Confirmation() { }
    public Confirmation(DiscordMessageId confirmMessage, DiscordUserId requestedBy, DiscordUserId? confirmedBy, IConfirmCommand confirmCommand, bool isConfirmed = false, bool isDenied = false) {
        ConfirmMessage = confirmMessage;
        RequestedBy = requestedBy;
        ConfirmedBy = confirmedBy;
        ConfirmCommand = confirmCommand;
        IsConfirmed = isConfirmed;
        IsDenied = isDenied;
    }
    public DiscordMessageId ConfirmMessage { get; init; }
    public DiscordUserId RequestedBy { get; init; }
    public DiscordUserId? ConfirmedBy { get; init; }
    public IConfirmCommand ConfirmCommand { get; init; }
    public bool IsConfirmed { get; init; }
    public bool IsDenied { get; init; }

    public void Deconstruct(out DiscordMessageId confirmMessage, out DiscordUserId requestUser, out DiscordUserId? confirmedBy, out IConfirmCommand confirmData, out bool isConfirmed, out bool isDenied) {
        confirmMessage = ConfirmMessage;
        requestUser = RequestedBy;
        confirmedBy = ConfirmedBy;
        confirmData = ConfirmCommand;
        isConfirmed = IsConfirmed;
        isDenied = IsDenied;
    }
}
