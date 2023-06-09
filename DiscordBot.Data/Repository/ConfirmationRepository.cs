using DiscordBot.Common.Models.Data.Confirmation;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository;

internal sealed class ConfirmationRepository : BaseRecordLiteDbRepository<Confirmation>, IConfirmationRepository {
    public ConfirmationRepository(ILogger logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => "confirmations";

    public Result<Confirmation> GetUnconfirmedByMessageId(DiscordMessageId discordMessageId) {
        var stuff = GetCollection()
            .Query()
            .Where(x => x.ConfirmMessage == discordMessageId && x.IsConfirmed == false && x.IsDenied == false)
            .FirstOrDefault();
        
        

        return Result.Ok(stuff);
    }
}
