using System;
using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services {
    public class AutomatedDropperService : BaseService, IAutomatedDropperService {
        public AutomatedDropperService(ILogger<AutomatedDropperService> logger) : base(logger) { }

        public Task<Result> HandleDrop(RunescapeDrop drop, string base64Image) {
            throw new NotImplementedException();
        }
    }
}
