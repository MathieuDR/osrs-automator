using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository {
    public class GuildConfigRepository : BaseLiteDbRepository<GuildConfig>, IGuildConfigRepository{
        public GuildConfigRepository(ILogger<GuildConfigRepository> logger, LiteDatabase database) : base(logger, database) { }
    }
}
