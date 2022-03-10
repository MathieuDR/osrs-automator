using System.ComponentModel.DataAnnotations;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Services.Services;

internal class PlayerService : RepositoryService, IPlayerService {
    private readonly IDiscordService _discordService;
    private readonly IOsrsHighscoreService _osrsHighscoreService;

    public PlayerService(ILogger<PlayerService> logger, IDiscordService discordService, IOsrsHighscoreService osrsHighscoreService,
        IRepositoryStrategy repositoryStrategy) :
        base(logger, repositoryStrategy) {
        _discordService = discordService;
        _osrsHighscoreService = osrsHighscoreService;
    }

    public async Task<ItemDecorator<Player>> CoupleDiscordGuildUserToOsrsAccount(GuildUser user,
        string proposedOsrsName) {
        proposedOsrsName = proposedOsrsName.ToLowerInvariant();
        var repo = GetRepository<IPlayerRepository>(user.GuildId);
        var discordUserPlayer = repo.GetByDiscordId(user.Id).Value ?? new Common.Models.Data.PlayerManagement.Player(user.GuildId, user.Id);

        CheckIfPlayerIsAlreadyCoupled(user, proposedOsrsName, discordUserPlayer);

        var osrsPlayer = await _osrsHighscoreService.GetPlayersForUsername(proposedOsrsName);
        //  Check if coupled with other user by ID
        if (IsPresentById(discordUserPlayer.CoupledOsrsAccounts, osrsPlayer.Id)) {
            // Already coupled to this account
            UpdateExistingPlayerOsrsAccount(user.GuildId, discordUserPlayer, osrsPlayer);
            return osrsPlayer.Decorate();
        }

        if (IsIdCoupledInServer(user.GuildId, osrsPlayer.Id)) {
            throw new ValidationException($"User {proposedOsrsName} is already registered on this server.");
        }

        await AddNewOsrsAccount(user, discordUserPlayer, osrsPlayer);
        return osrsPlayer.Decorate();
    }

    public Task<IEnumerable<ItemDecorator<Player>>> GetAllOsrsAccounts(GuildUser user) {
        var repo = GetRepository<IPlayerRepository>(user.GuildId);
        var player = repo.GetByDiscordId(user.Id).Value;

        if (player == null) {
            return Task.FromResult(new List<ItemDecorator<Player>>().AsEnumerable());
        }

        var accounts = player.CoupledOsrsAccounts;
        var tasks = new List<Task>();

        for (var i = 0; i < accounts.Count; i++) {
            var index = i;
            var account = accounts[i];

            // Account is 7 days old, or doesn't have a snapshot.
            var task = _osrsHighscoreService.GetPlayerById(account.Id).ContinueWith(antecedent => {
                var p = antecedent.Result;
                if (p != null) {
                    accounts[index] = p;
                }
            });

            tasks.Add(task);
        }

        if (tasks.Any()) {
            Task.WaitAll(tasks.ToArray());

            player.CoupledOsrsAccounts = accounts.ToList();
            player.DefaultPlayerUsername =
                accounts.Where(x => x.Id == player.WiseOldManDefaultPlayerId).Select(x => x.DisplayName)
                    .FirstOrDefault() ?? player.DefaultPlayerUsername;

            repo.UpdateOrInsert(player);
        }

        return Task.FromResult(accounts.Decorate());
    }

    public Task<NameChange> RequestNameChange(int womAccountId, string newName) {
        throw new NotImplementedException("Pls implement!");
    }

    public Task<NameChange> RequestNameChange(string oldName, string newName) {
        return _osrsHighscoreService.RequestNameChange(oldName, newName);
    }

    public async Task DeleteCoupledOsrsAccount(GuildUser user, int id) {
        var player = GetPlayerConfigurationOrThrowException(user);

        if (player.CoupledOsrsAccounts.Count == 1) {
            throw new Exception("Cannot delete last coupled account. Please add a new one first.");
        }

        var index = player.CoupledOsrsAccounts.FindIndex(x => x.Id == id);
        player.CoupledOsrsAccounts.RemoveAt(index);

        if (player.WiseOldManDefaultPlayerId == id) {
            await SetDefaultAccount(user, player.CoupledOsrsAccounts.FirstOrDefault(), player);
        }

        var repo = GetRepository<IPlayerRepository>(user.GuildId);
        repo.UpdateOrInsert(player);
    }

    public Task<string> SetDefaultAccount(GuildUser user, Player player) {
        return SetDefaultAccount(user, player, null);
    }

    public Task<string> GetDefaultOsrsDisplayName(GuildUser user) {
        var repo = GetRepository<IPlayerRepository>(user.GuildId);
        var player = repo.GetByDiscordId(user.Id).Value;
        return Task.FromResult(player?.DefaultPlayerUsername);
    }

    public Task<string> GetUserNickname(GuildUser user, out bool isOsrsAccount) {
        var player = GetPlayerConfigurationOrThrowException(user);
        isOsrsAccount = string.IsNullOrEmpty(player.Nickname);
        return isOsrsAccount ? Task.FromResult(player.DefaultPlayerUsername) : Task.FromResult(player.Nickname);
    }

    public async Task<string> SetUserName(GuildUser user, string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            throw new Exception("Name must not be empty!");
        }

        var player = GetPlayerConfigurationOrThrowException(user);
        player.Nickname = name;

        await EnforceUsername(user, player);

        var repo = GetRepository<IPlayerRepository>(user.GuildId);
        repo.UpdateOrInsert(player);

        return player.Nickname;
    }

    private bool IsPresentById(List<Player> players, int id) {
        return players.Any(p => p.Id == id);
    }

    private bool IsIdCoupledInServer(DiscordGuildId guildId, int id) {
        var repo = GetRepository<IPlayerRepository>(guildId);
        return repo.GetPlayerByOsrsAccount(id) != null;
    }

    public async Task<string> SetDefaultAccount(GuildUser discordUser, Player osrsPlayer, Common.Models.Data.PlayerManagement.Player player) {
        if (player == null) {
            player = GetPlayerConfigurationOrThrowException(discordUser);
        }

        if (player.CoupledOsrsAccounts.All(x => x.Id != osrsPlayer.Id)) {
            // Should never really happen though..
            await AddNewOsrsAccount(discordUser, player, osrsPlayer);
        }

        player.DefaultPlayerUsername = osrsPlayer.DisplayName;
        player.WiseOldManDefaultPlayerId = osrsPlayer.Id;

        await EnforceUsername(discordUser, player);

        var repo = GetRepository<IPlayerRepository>(discordUser.GuildId);

        repo.UpdateOrInsert(player);
        return player.DefaultPlayerUsername;
    }

    private async Task EnforceUsername(GuildUser user, Common.Models.Data.PlayerManagement.Player playerConfig) {
        if (playerConfig.EnforceNameTemplate && !string.IsNullOrWhiteSpace(playerConfig.DefaultPlayerUsername) &&
            !string.IsNullOrWhiteSpace(playerConfig.Nickname)) {
            var result = await _discordService.SetUsername(user, $"{playerConfig.DefaultPlayerUsername} ({playerConfig.Nickname})");
        }
    }

    private Common.Models.Data.PlayerManagement.Player GetPlayerConfigurationOrThrowException(GuildUser user) {
        var repo = GetRepository<IPlayerRepository>(user.GuildId);
        var config = repo.GetByDiscordId(user.Id).Value;

        if (config == null) {
            throw new Exception("No configuration found for player!");
        }

        return config;
    }

    private async Task AddNewOsrsAccount(GuildUser discordUser, Common.Models.Data.PlayerManagement.Player player, Player osrsPlayer) {
        // New account
        player.CoupledOsrsAccounts.Add(osrsPlayer);

        if (player.CoupledOsrsAccounts.Count == 1) {
            // RISKY LOOP!
            await SetDefaultAccount(discordUser, osrsPlayer, player);
        }

        var repo = GetRepository<IPlayerRepository>(discordUser.GuildId);
        repo.UpdateOrInsert(player);

        var configRepo = GetRepository<IGuildConfigRepository>(discordUser.GuildId);
        var config = configRepo.GetSingle().Value;
        if (config is not null && config.AutoAddNewAccounts) {
            await _osrsHighscoreService.AddOsrsAccountToToGroup(config.WomGroupId, config.WomVerificationCode, osrsPlayer.Username);
        }
    }

    private void CheckIfPlayerIsAlreadyCoupled(GuildUser discordUser, string proposedOsrsName, Common.Models.Data.PlayerManagement.Player player) {
        if (player.CoupledOsrsAccounts.Any(p => p.Username == proposedOsrsName)) {
            throw new ValidationException($"User {proposedOsrsName} is already coupled to you.");
        }

        var repo = GetRepository<IPlayerRepository>(discordUser.GuildId);
        if (repo.GetPlayerByOsrsAccount(proposedOsrsName) != null) {
            throw new ValidationException($"User {proposedOsrsName} is already registered on this server.");
        }
    }

    private void UpdateExistingPlayerOsrsAccount(DiscordGuildId guildId, Common.Models.Data.PlayerManagement.Player toUpdate, Player osrsPlayer) {
        var old = toUpdate.CoupledOsrsAccounts.Find(p => p.Id == osrsPlayer.Id);
        toUpdate.CoupledOsrsAccounts.Remove(old);
        toUpdate.CoupledOsrsAccounts.Add(osrsPlayer);

        var repo = GetRepository<IPlayerRepository>(guildId);
        repo.UpdateOrInsert(toUpdate);
    }
}
