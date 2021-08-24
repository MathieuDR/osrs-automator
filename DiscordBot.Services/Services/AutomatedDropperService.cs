using System;
using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services {
    public class AutomatedDropperService : BaseService, IAutomatedDropperService {
    

        public Task<Result> HandleDrop(RunescapeDrop drop, string base64Image) {
            throw new NotImplementedException();
        }

        public AutomatedDropperService(ILogger<AutomatedDropperService> logger, RepositoryStrategy repositoryStrategy) : base(logger, repositoryStrategy) { }
    }
}
