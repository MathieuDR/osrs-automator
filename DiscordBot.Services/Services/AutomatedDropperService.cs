using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DiscordBot.Services.Services;

internal class AutomatedDropperService : RepositoryService, IAutomatedDropperService {
    private readonly ISchedulerFactory _schedulerFactory;

    public AutomatedDropperService(ILogger<AutomatedDropperService> logger, IRepositoryStrategy repositoryStrategy,
        ISchedulerFactory schedulerFactory) : base(logger, repositoryStrategy) {
        _schedulerFactory = schedulerFactory;
    }

    public async Task<Result<string>> RequestUrl(GuildUser user) {
        // get RunescapeDropperGuildConfiguration
        var guildConfigurationResult = GetGuildConfiguration(user.GuildId, user.Id);
        if (guildConfigurationResult.IsFailed) {
            return Result.Fail<string>("Could not get your endpoint")
                .WithErrors(guildConfigurationResult.Errors);
        }
        
        var guildConfiguration = guildConfigurationResult.Value;
        
        // check if user is disabled
        if (guildConfiguration.DisabledUsers.Contains(user.Id)) {
            return Result.Fail<string>("You are unable to use this service, you have been disabled by an admin");
        }
        
        // check if user already has an endpoint
        if(guildConfiguration.UserEndpoints.ContainsKey(user.Id)) {
            // We would like to reset
            guildConfiguration.UserEndpoints.Remove(user.Id);
        }
        
        // get a new endpoint
        var endpoint = EndpointId.New();
        guildConfiguration.UserEndpoints.Add(user.Id, endpoint);
        
        // save the endpoint
        var saveResult = SaveGuildConfiguration(guildConfiguration);
        if (saveResult.IsFailed) {
            return Result.Fail<string>("Could not save your endpoint to the database")
                .WithErrors(guildConfigurationResult.Errors);
        }
        
        return Result.Ok(CreateEndpointUri(endpoint));
    }

    public Task<Result<DropperGuildConfiguration>> GetGuildConfiguration(Guild guild) =>
        Task.FromResult(GetGuildConfiguration(guild.Id, DiscordUserId.Empty));

    Task<Result> IAutomatedDropperService.SaveGuildConfiguration(DropperGuildConfiguration configuration) {
        var repo = RepositoryStrategy.GetOrCreateRepository<IRunescapeDropperGuildConfigurationRepository>(configuration.GuildId);
        var r= repo.UpdateOrInsert(configuration);
        return Task.FromResult<Result>(r);
    }

    private Result SaveGuildConfiguration(DropperGuildConfiguration guildConfiguration) {
        var repo = RepositoryStrategy.GetOrCreateRepository<IRunescapeDropperGuildConfigurationRepository>(guildConfiguration.GuildId);
        return repo.UpdateOrInsert(guildConfiguration);
    }

    private string CreateEndpointUri(EndpointId endpoint) {
        // create uri
        var uri = new UriBuilder("https", "localhost", 80, $"api/v1.0/AutomatedDropper/dropper/{endpoint.Value}");
        return uri.ToString();
    }

    private Result<DropperGuildConfiguration> GetGuildConfiguration(DiscordGuildId guildId, DiscordUserId requestingUser) {
        var repo = RepositoryStrategy.GetOrCreateRepository<IRunescapeDropperGuildConfigurationRepository>(guildId);
        var result = repo.GetSingle();

        if (result.IsFailed) {
            return result;
        }

        if (result.Value is null) {
            // TODO: Remvo the create
            return Result.Ok(new DropperGuildConfiguration(guildId, requestingUser));
            return Result.Fail("No RunescapeDropperGuildConfiguration found");
        }
        
        return Result.Ok(result.Value);
    }

    public async Task<Result> HandleDropRequest(EndpointId endpointId, RunescapeDrop drop, string base64Image) {
        if (drop is null && string.IsNullOrEmpty(base64Image)) {
            return Result.Fail("No new information");
        }

        //Check user Id
        var allowedCheckResult = IsValidEndpoint(endpointId);
        if (allowedCheckResult.IsFailed) {
            Logger.LogInformation("Not allowed endpoint: {endpoint}", endpointId.ToString());
            return allowedCheckResult.ToResult();
        }

        var userId = allowedCheckResult.Value;

        //Save to DB
        var saveInformationResult = SaveDropData(endpointId, userId, drop, base64Image);
        if (saveInformationResult.IsFailed) {
            Logger.LogWarning("Could not save drop with endpoint {endpoint}, data: {@drop} and {verb} image", endpointId.ToString(), drop,
                string.IsNullOrEmpty(base64Image) ? "without" : "with");
            return saveInformationResult.ToResult();
        }

        //Schedule job // hook
        return Result.Ok();
    }

    private Result<RunescapeDrop> SaveDropData(EndpointId endpoint, DiscordUserId userId, RunescapeDrop drop, string base64Image) {
   
    }
   

    private Result<DiscordUserId> IsValidEndpoint(EndpointId endpointId) {
        return Result.Ok(DiscordUserId.Empty);
    }
}
