using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository {
    public class AutomatedJobStateRepository: BaseLiteDbRepository<AutomatedJobState>, IAutomatedJobStateRepository {
        public override string CollectionName => "guildJobState";
        public AutomatedJobStateRepository(ILogger<AutomatedJobStateRepository> logger, LiteDatabase database) : base(logger, database) { }
    }
}