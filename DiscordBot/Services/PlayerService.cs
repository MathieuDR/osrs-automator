using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Helpers;
using DiscordBot.Models.Decorators;
using DiscordBot.Repository;
using DiscordBot.Services.interfaces;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Services {
    public class PlayerService : BaseService, IPlayerService {
        private readonly IOsrsHighscoreService _osrsHighscoreService;
        private readonly IDiscordBotRepository _repository;

        public PlayerService(ILogService logger, IOsrsHighscoreService osrsHighscoreService, IDiscordBotRepository repository) :
            base(logger) {
            _osrsHighscoreService = osrsHighscoreService;
            _repository = repository;
        }

        public async Task<ItemDecorator<Player>> CoupleDiscordGuildUserToOsrsAccount(IGuildUser discordUser,
            string proposedOsrsName) {
            proposedOsrsName = proposedOsrsName.ToLowerInvariant();

            var player = _repository.GetPlayerById(discordUser.GuildId, discordUser.Id) ?? new Models.Data.Player(discordUser);
            CheckIfPlayerIsAlreadyCoupled(discordUser, proposedOsrsName, player);

            var osrsPlayer = await _osrsHighscoreService.GetPlayersForUsername(proposedOsrsName);

            if (player.CoupledOsrsAccounts.Any(p => p.Id == osrsPlayer.Id)) {
                // Already coupled to this account
                UpdateExistingPlayerOsrsAccount(discordUser.GuildId, player, osrsPlayer);
                return osrsPlayer.Decorate();
            }

            var coupledUser = _repository.GetPlayerByOsrsAccount(discordUser.GuildId, osrsPlayer.Id);

            if (coupledUser != null) {
                //UpdateExistingPlayerOsrsAccount(discordUser.GuildId, coupledUser, osrsPlayer);
                throw new ValidationException($"User {proposedOsrsName} is already registered on this server.");
            }

            AddNewOsrsAccount(discordUser, player, osrsPlayer);
            return osrsPlayer.Decorate();
        }

        public Task<IEnumerable<ItemDecorator<Player>>> GetAllOsrsAccounts(IGuildUser user) {
            var player = _repository.GetPlayerById(user.GuildId, user.Id);

            if (player == null) {
                return Task.FromResult(new List<ItemDecorator<Player>>().AsEnumerable());
            }

            var accounts = player.CoupledOsrsAccounts;
            var tasks = new List<Task>();

            for (var i = 0; i < accounts.Count; i++) {
                var index = i;
                var account = accounts[i];

                //if (account.UpdatedAt.AddDays(7) > DateTimeOffset.Now && account.LatestSnapshot != null) {
                //    continue;
                //}

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
                _repository.UpdateOrInsertPlayerForGuild(user.GuildId, player);
            }

            return Task.FromResult(accounts.Decorate());
        }

        public Task DeleteCoupledOsrsAccount(IGuildUser user, int id) {
            var player = GetPlayerConfigurationOrThrowException(user);

            if (player.CoupledOsrsAccounts.Count == 1) {
                throw new Exception($"Cannot delete last coupled account. Please add a new one first.");
            }

            var index = player.CoupledOsrsAccounts.FindIndex(x => x.Id == id);
            player.CoupledOsrsAccounts.RemoveAt(index);

            if (player.WiseOldManDefaultPlayerId == id) {
                SetDefaultAccount(user, player.CoupledOsrsAccounts.FirstOrDefault(), player);
            }

            _repository.UpdateOrInsertPlayerForGuild(user.GuildId, player);
            return Task.CompletedTask;
        }

        public Task<string> SetDefaultAccount(IGuildUser user, Player player) {
            return SetDefaultAccount(user, player, null);
        }

        public Task<string> GetDefaultOsrsDisplayName(IGuildUser user) {
            var player = _repository.GetPlayerById(user.GuildId, user.Id);
            return Task.FromResult(player.DefaultPlayerUsername);
        }

        public Task<string> GetUserNickname(IGuildUser user, out bool isOsrsAccount) {
            var player = GetPlayerConfigurationOrThrowException(user);
            isOsrsAccount = string.IsNullOrEmpty(player.Nickname);
            return isOsrsAccount ? Task.FromResult(player.DefaultPlayerUsername) : Task.FromResult(player.Nickname);
        }

        public async Task<string> SetUserName(IGuildUser user, string name) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new Exception($"Name must not be empty!");
            }

            var player = GetPlayerConfigurationOrThrowException(user);
            player.Nickname = name;

            await EnforceUsername(user, player);

            _repository.UpdateOrInsertPlayerForGuild(user.GuildId, player);
            return player.Nickname;
        }

        public async Task<string> SetDefaultAccount(IGuildUser discordUser, Player osrsPlayer, Models.Data.Player player) {
            if (player == null) {
                player = GetPlayerConfigurationOrThrowException(discordUser);
            }

            if (player.CoupledOsrsAccounts.All(x => x.Id != osrsPlayer.Id)) {
                // Should never really happen though..
                AddNewOsrsAccount(discordUser, player, osrsPlayer);
            }

            player.DefaultPlayerUsername = osrsPlayer.DisplayName;
            player.WiseOldManDefaultPlayerId = osrsPlayer.Id;

            await EnforceUsername(discordUser, player);

            _repository.UpdateOrInsertPlayerForGuild(discordUser.GuildId, player);
            return player.DefaultPlayerUsername;
        }

        private async Task EnforceUsername(IGuildUser user, Models.Data.Player playerConfig) {
            if (playerConfig.EnforceNameTemplate && !string.IsNullOrWhiteSpace(playerConfig.DefaultPlayerUsername) &&
                !string.IsNullOrWhiteSpace(playerConfig.Nickname)) {
                await user.ModifyAsync(u => u.Nickname = $"{playerConfig.DefaultPlayerUsername} ({playerConfig.Nickname})");
            }
        }

        private Models.Data.Player GetPlayerConfigurationOrThrowException(IGuildUser user) {
            var config = _repository.GetPlayerById(user.GuildId, user.Id);

            if (config == null) {
                throw new Exception($"No configuration found for player!");
            }

            return config;
        }

        private void AddNewOsrsAccount(IGuildUser discordUser, Models.Data.Player player, Player osrsPlayer) {
            // New account
            player.CoupledOsrsAccounts.Add(osrsPlayer);

            if (player.CoupledOsrsAccounts.Count == 1) {
                // RISKY LOOP!
                SetDefaultAccount(discordUser, osrsPlayer, player);
            }

            _repository.UpdateOrInsertPlayerForGuild(discordUser.GuildId, player);

            var config = _repository.GetGroupConfig(discordUser.GuildId);
            if (config.AutoAddNewAccounts) {
                _osrsHighscoreService.AddOsrsAccountToToGroup(config.WomGroupId, config.WomVerificationCode, osrsPlayer.Username);
            }
        }

        private void CheckIfPlayerIsAlreadyCoupled(IGuildUser discordUser, string proposedOsrsName, Models.Data.Player player) {
            if (player.CoupledOsrsAccounts.Any(p => p.Username == proposedOsrsName)) {
                throw new ValidationException($"User {proposedOsrsName} is already coupled to you.");
            }

            if (_repository.GetPlayerByOsrsAccount(discordUser.GuildId, proposedOsrsName) != null) {
                throw new ValidationException($"User {proposedOsrsName} is already registered on this server.");
            }
        }

        private void UpdateExistingPlayerOsrsAccount(ulong guildId, Models.Data.Player toUpdate, Player osrsPlayer) {
            var old = toUpdate.CoupledOsrsAccounts.Find(p => p.Id == osrsPlayer.Id);
            toUpdate.CoupledOsrsAccounts.Remove(old);
            toUpdate.CoupledOsrsAccounts.Add(osrsPlayer);

            _repository.UpdateOrInsertPlayerForGuild(guildId, toUpdate);
        }
    }
}