using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services {
    public class CompetitionService : ICompetitionService{
        private readonly IDiscordBotRepository _repository;
        private readonly IOsrsHighscoreService _highscoreService;

        public CompetitionService(IDiscordBotRepository repository, IOsrsHighscoreService highscoreService) {
            _repository = repository;
            _highscoreService = highscoreService;
        }

        public async Task<IEnumerable<ItemDecorator<Competition>>> ViewCompetitionsForGroup(IGuildUser guildUser) {
            var allCompetitions = await ViewAllCompetitionsForGroup(guildUser);

            // Filter active ones
            var result = allCompetitions.Where(x => x.Item.EndDate > DateTimeOffset.Now);

            return result;
        }

        public async Task<IEnumerable<ItemDecorator<Competition>>> ViewAllCompetitionsForGroup(IGuildUser guildUser) {
            var config = GetGroupConfig(guildUser.GuildId);

            var competitions = await _highscoreService.GetAllCompetitionsForGroup(config.WomGroupId);
            return competitions.Decorate();
        }


        private GroupConfig GetGroupConfig(ulong guildId) {
            var config = _repository.GetGroupConfig(guildId);

            if (config == null) {
                throw new Exception("No group set for server.");
            }

            return config;
        }

        public async Task<ItemDecorator<Competition>> SetCurrentCompetition(IGuildUser guildUser, int id) {
            // Do we really need this?
            throw new NotImplementedException();

            /*Competition competition = await _highscoreService.GetCompetitionById(id);

            if (competition == null) {
                throw new Exception("Competition does not exist.");
            }

            var config = GetGroupConfig(guildUser.GuildId);
            

            

            return null;
            */
            //var competition = _highscoreService.GetCompetition(id);
            //return competition;
        }

        public async Task<ItemDecorator<Competition>> SetCurrentCompetition(IGuildUser guildUser, string name) {
            throw new System.NotImplementedException();
        }

        public async Task<ItemDecorator<Competition>> ViewCurrentCompetition(IGuild guild) {
            throw new System.NotImplementedException();
        }
    }
}