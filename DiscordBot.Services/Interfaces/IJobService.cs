using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using FluentResults;

namespace DiscordBot.Services.Interfaces {
    public interface IJobService {
        Task<Result<ChannelJobConfiguration>> GetConfigurationForJobType(JobType job);
    }
}
