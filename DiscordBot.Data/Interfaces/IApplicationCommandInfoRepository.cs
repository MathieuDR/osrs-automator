using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using FluentResults;

namespace DiscordBot.Data.Interfaces {
    public interface IApplicationCommandInfoRepository: IRecordRepository<ApplicationCommandInfo> {
        Task<Result<ApplicationCommandInfo>> GetByCommandName(string command);
    }
}
