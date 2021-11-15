using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Jobs; 

public class HandleRunescapeDropJob : RepositoryJob {
    private readonly IDiscordService _discordService;
    private readonly ICollectionLogItemProvider _collectionLogItemProvider;

    public HandleRunescapeDropJob(ILogger<HandleRunescapeDropJob> logger, 
        IRepositoryStrategy repositoryStrategy, 
        IDiscordService discordService,
        ICollectionLogItemProvider collectionLogItemProvider)
        : base(logger, repositoryStrategy) {
        _discordService = discordService;
        _collectionLogItemProvider = collectionLogItemProvider;
    }

    protected override async Task<Result> DoWork() {
        var endpoint = Context.MergedJobDataMap.GetGuidValue("endpoint");
        var repo = RepositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>();
        var data = repo.GetActive(endpoint).Value;
            
        var guildIds = GetGuildIdsForEndpoint(endpoint).ToList();

        if (!guildIds.Any()) {
            // We don't handle anything with no active guilds
            repo.Delete(data);
            return Result.Ok();
        }

        data = SetImagesForAllDrops(data);
        var messagesResult = await HandleMessagesForGuilds(endpoint, data, guildIds);
        if (messagesResult.IsFailed) {
            return messagesResult.ToResult();
        }

        if (!messagesResult.Value) {
            // No messages sent, we can delete this too
            repo.Delete(data);
        }

        repo.CloseActive(endpoint);
        return Result.Ok();
    }

    private RunescapeDropData SetImagesForAllDrops(RunescapeDropData data) {
        var imageTemp = data.Drops.FirstOrDefault(x => x.Image is not null)?.Image;
        if (imageTemp is null) {
            // No images, nothing to fix
            return data;
        }

        var drops = data.Drops.ToList();
        for (var i = 0; i < drops.Count; i++) {
            var drop = drops[i];
            if (drop.Image is not null) {
                imageTemp = drop.Image;
                continue;
            }

            drop = drop with {Image = imageTemp};
            drops.Insert(i, drop);
            drops.RemoveAt(i + 1);
        }

        return data with {Drops = drops};
    }

    private async Task<Result<bool>> HandleMessagesForGuilds(Guid endpoint, RunescapeDropData data, List<ulong> guildIds) {
        var errors = new List<IError>();
        bool sentAnyMessages = false;
            
        // Set the handled to a list that we can edit
        var messagedGuilds = data.GuildsMessaged.ToList();
        data = data with {GuildsMessaged = messagedGuilds};

        // Handle all messages
        foreach (var guildId in guildIds) {
            if (data.GuildsMessaged.Contains(guildId)) {
                continue;
            }

            var repo = RepositoryStrategy.GetOrCreateRepository<IRunescapeDropperGuidConfigurationRepository>(guildId);
            var configurationResult = repo.GetSingle();
            if (configurationResult.IsFailed) {
                errors.AddRange(configurationResult.Errors);
                continue;
            }

            foreach (var channelConfiguration in configurationResult.Value.EnabledChannels) {
                var filteredDada = await FilterData(data, channelConfiguration);
                sentAnyMessages = sentAnyMessages || SendData(guildId, channelConfiguration.Channel, filteredDada);
            }

            messagedGuilds.Add(guildId);
        }
            
        return Result.FailIf(!errors.Any(),"Some guilds failed").WithErrors(errors).ToResult(sentAnyMessages);
    }

    private bool SendData(ulong guildId, ulong channelId, RunescapeDropData toSendData) {
        if (toSendData is null) {
            return false;
        }

        _discordService.PrintRunescapeDataDrop(toSendData, guildId, channelId);
        return true;
    }

    private IEnumerable<ulong> GetGuildIdsForEndpoint(Guid endpoint) {
        return new ulong[] {403539795944538122};
    }

    private async Task<RunescapeDropData> FilterData(RunescapeDropData data, RunescapeDropperChannelConfiguration configuration) {
        if (data.Drops.Any(x=>x.IsPet)) {
            return data with {};
        }

        var filtered = new List<RunescapeDrop>();

        if(configuration.WhiteListEnabled) {
            // If it's enabled.
            // Only use whitelist
            filtered = data.Drops.Where(x => configuration.WhiteListedItems.Contains(x.Item?.Name)).ToList();
            return data with {Drops = filtered};
        }

        var drops = data.Drops.ToList();
        var collectionLogItems = (await _collectionLogItemProvider.GetCollectionLogItemNames()).Value.Select(x=>x.ToLowerInvariant()).ToList();

        for (var i = 0; i < drops.Count; i++) {
            var drop = drops[i];
            var itemName = drop.Item?.Name.ToLowerInvariant();

            if (configuration.BlackListedItems.Contains(itemName)) {
                // Blacklisted.
                // We do not want it
                continue;
            }

            var maxValue = Math.Max(drop.TotalValue,drop.TotalHaValue);

            if (configuration.UseCollectionLogExceptions && collectionLogItems.Contains(itemName)) { 
                filtered.Add(drop);
                continue;
            }

            if ((configuration.MinimumValue <= maxValue && configuration.MinRarity <= drop.Rarity) || (configuration.OrOperator&&(configuration.MinimumValue <= maxValue || configuration.MinRarity <= drop.Rarity))) {
                filtered.Add(drop);
                continue;
            }
        }

        return data with {Drops = filtered};
    }
}