using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using FluentResults;
using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository; 

public class ApplicationCommandInfoRepository : BaseRecordLiteDbRepository<ApplicationCommandInfo>, IApplicationCommandInfoRepository {
    public ApplicationCommandInfoRepository(ILogger<ApplicationCommandInfoRepository> logger, LiteDatabase database) : base(logger, database) { }
    public override string CollectionName => nameof(ApplicationCommandInfo);
        
    public  Task<Result<ApplicationCommandInfo>> GetByCommandName(string command) {
        return Task.FromResult(Result.Ok(GetCollection().Query().Where(x=>x.CommandName == command).SingleOrDefault()));
    }
}