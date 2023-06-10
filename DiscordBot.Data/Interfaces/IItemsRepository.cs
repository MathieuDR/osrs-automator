using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Common.Models.Data.Items;

namespace DiscordBot.Data.Interfaces; 

internal interface IItemsRepository  : ISingleRecordRepository<SelfCountConfiguration> {
    
}
