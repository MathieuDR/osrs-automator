using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services {
    public class PlayerService :BaseService, IPlayerService {
        private readonly IOsrsHighscoreService _osrsHighscoreService;
        private readonly IDiscordBotRepository _repository;

        public PlayerService(ILogService logger, IOsrsHighscoreService osrsHighscoreService, IDiscordBotRepository repository) : base(logger) {
            _osrsHighscoreService = osrsHighscoreService;
            _repository = repository;
        }

        public async Task<Player> CoupleDiscordGuildUserToOsrsAccount(IGuildUser discordUser, string proposedOsrsName) {
            proposedOsrsName = proposedOsrsName.ToLowerInvariant();
            
            var player = _repository.GetPlayerById(discordUser.GuildId, discordUser.Id) ?? new Models.Data.Player(discordUser);
            CheckIfPlayerIsAlreadyCoupled(discordUser, proposedOsrsName, player);

            var osrsPlayer = await _osrsHighscoreService.GetPlayersForUsername(proposedOsrsName);

            if (player.CoupledOsrsAccounts.Any(p => p.Id == osrsPlayer.Id)) {
                // Already coupled to this account
                UpdateExistingPlayerOsrsAccount(discordUser.GuildId, player, osrsPlayer);
                return osrsPlayer;
            }

            var coupledUser = _repository.GetPlayerByOsrsAccount(discordUser.GuildId, osrsPlayer.Id);

            if (coupledUser != null) {
                UpdateExistingPlayerOsrsAccount(discordUser.GuildId, coupledUser, osrsPlayer);
                throw new ValidationException($"User {proposedOsrsName} is already registered on this server.");
            }

            AddNewOsrsAccount(discordUser, player, osrsPlayer);
            return osrsPlayer;
        }

        public Task<IEnumerable<Player>> GetAllOsrsAccounts(IGuildUser user) {
            var accounts = _repository.GetPlayerById(user.GuildId, user.Id).CoupledOsrsAccounts.AsEnumerable();
            return Task.FromResult(accounts);
        }

        public Task DeleteCoupleOsrsAccountAtIndex(IGuildUser user, int index) {
            var player = _repository.GetPlayerById(user.GuildId, user.Id);
            player.CoupledOsrsAccounts.RemoveAt(index);
            _repository.UpdateOrInsertPlayerForGuild(user.GuildId, player);
            return Task.CompletedTask;
        }

        private void AddNewOsrsAccount(IGuildUser discordUser, Models.Data.Player player, Player osrsPlayer) {
            // New account
            player.CoupledOsrsAccounts.Add(osrsPlayer);
            if (player.CoupledOsrsAccounts.Count == 1) {
                player.DefaultPlayerUsername = osrsPlayer.Username;
                player.WiseOldManDefaultPlayerId = osrsPlayer.Id;
            }

            _repository.UpdateOrInsertPlayerForGuild(discordUser.GuildId, player);
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