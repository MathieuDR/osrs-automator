using DiscordBot.Common.Models.Data.Confirmation;
using FluentResults;

namespace DiscordBot.Data.Interfaces;

public interface IConfirmationRepository : IRecordRepository<Confirmation> {
    Result<Confirmation> GetUnconfirmedByMessageId(DiscordMessageId discordMessageId);
}
