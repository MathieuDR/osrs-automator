using DiscordBot.Common.Models.Data.Items;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

internal sealed class ItemsRepository : BaseSingleRecordLiteDbRepository<GuildItems>, IItemsRepository{
    public ItemsRepository(ILogger logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => "items";
}
