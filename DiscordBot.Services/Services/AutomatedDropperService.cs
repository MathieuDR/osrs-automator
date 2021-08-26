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
            // Save to DB
            // Set or update job with ID: {recipient:username}
            // If Job already exist, reset timer + Options
            // Timer: 1 sec, Clue's: 30 sec
            throw new NotImplementedException();
        }
        
        // Job
        // Gather all of username
        // Filter
        // -- Filtered keywords
        // --- Total value/HA
        // --- Check for collection log item
        // if exceeds for a post?
        // --create post
        // --send post to all discord guilds where that username is registered (atm all)
        // remove from db, end Job

        public AutomatedDropperService(ILogger<AutomatedDropperService> logger, IRepositoryStrategy repositoryStrategy) : base(logger, repositoryStrategy) { }
    }
}
