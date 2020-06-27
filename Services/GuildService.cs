using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Requests;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
    public class GuildService : IGuildService{
        private readonly IDiscordBotRepository _repository;
        private readonly IHighscoreApiRepository _highscoreApiRepository;
        private readonly WiseOldManConfiguration _configuration;

        public GuildService(IDiscordBotRepository repository, IHighscoreApiRepository highscoreApiRepository, WiseOldManConfiguration configuration) {
            _repository = repository;
            _highscoreApiRepository = highscoreApiRepository;
            _configuration = configuration;
        }
        
        public bool HasActiveEvent(IGuild guild) {
            return _repository.GetAllActiveGuildEvents(guild.Id).Any();
        }
        public List<GuildCustomEvent> GetActiveGuildEvents(IGuild guild) {
            return _repository.GetAllActiveGuildEvents(guild.Id).ToList();
        }

        public GuildCustomEvent InsertGuildEvent(GuildCustomEvent guildCustomEvent) {
            guildCustomEvent.IsValid();

            return _repository.InsertGuildEvent(guildCustomEvent);
        }

        public bool EndGuildEvent(GuildCustomEvent guildCustomEvent) {
            guildCustomEvent.EndTime = DateTime.Now;
            _repository.UpdateGuildEvent(guildCustomEvent);
            return true;
        }

        public void AssertUserInGuild(IGuildUser user) {
            if (user == null) {
                throw new AuthenticationException($"Could not find guild for user. Are you using a guild command in a direct message?");
            }
        }

        public Task FixPlayerForCompetition(IGuild guild, string username, int offset) {
            var comps = _repository.GetAllActiveGuildCompetitions(guild.Id);
            if (comps.Count != 1) {
                throw new Exception($"No Competition running or multiple running.");
            }

            var competition = comps.FirstOrDefault();
            competition.Offsets[username.ToLowerInvariant()] = offset;
            _repository.UpdateGuildCompetition(competition);

            return Task.CompletedTask;
        }

        public bool DoesUserHavePermission(IGuildUser user, Permissions permission) {
            AssertUserInGuild(user);

            if (user.GuildPermissions.Administrator) {
                // admins always have permission
                return true;
            }
            
            GuildConfiguration guildConfiguration = _repository.GetGuildConfigurationById(user.Guild.Id);

            if (guildConfiguration == null) {
                return false;
            }

            return guildConfiguration.GuildRoles.Any(x => user.RoleIds.Contains(x.RoleId));

        }

        public bool ToggleRole(IRole role, Permissions permission) {
            GuildConfiguration guildConfiguration = _repository.GetGuildConfigurationById(role.Guild.Id);

            if (guildConfiguration == null) {
                guildConfiguration = new GuildConfiguration(){GuildId =  role.Guild.Id, GuildRoles = new List<GuildRole>()};
            }

            GuildRole roleConfig = guildConfiguration.GuildRoles.FirstOrDefault(x => x.RoleId == role.Id);
            if (roleConfig == null) {
                roleConfig = new GuildRole(){RoleId = role.Id, PermissionFlags = 0};
                guildConfiguration.GuildRoles.Add(roleConfig);
            }

            Permissions rolePermissions = (Permissions) roleConfig.PermissionFlags;
            bool hasPermission = rolePermissions.HasFlag(permission);

            if (hasPermission) {
                roleConfig.PermissionFlags -= (int) permission;
            } else {
                roleConfig.PermissionFlags += (int) permission;
            }

            _repository.UpdateOrInsertGuildConfiguration(guildConfiguration);
            return !hasPermission;
        }

        public bool AddEventCounter(IGuild guild, UserListWithImageArguments arguments) {
            var guildEvent = GetActiveGuildEvents(guild).SingleOrDefault();
            if (guildEvent == null) {
                throw new NullReferenceException($"Cannot find an active event!");
            }

            // Check if we have enough users
            if (arguments.Users.Count() >= guildEvent.MinimumPerCounter && arguments.Users.Count() <= guildEvent.MaximumPerCounter) {
                guildEvent.EventCounters??=new List<GuildEventCounter>();
                
                foreach (IUser user in arguments.Users) {
                    // Check if user has image already
                    if (!guildEvent.EventCounters.Any(x => x.UserId == user.Id && x.ImageUrl == arguments.ImageUrl)) {
                        guildEvent.EventCounters.Add(new GuildEventCounter(){UserId = user.Id, ImageUrl = arguments.ImageUrl,});
                    }
                }

                return _repository.UpdateGuildEvent(guildEvent) != null;
            } else {
                throw new ArgumentException($"User count is wrong! Please use the `help` command and also check how many users the event require!");
            }
        }

        public void RemoveCounters(GuildCustomEvent guildCustomEvent, List<GuildEventCounter> toDelete) {
            guildCustomEvent.EventCounters = guildCustomEvent.EventCounters.Except(toDelete).ToList();
            _repository.UpdateGuildEvent(guildCustomEvent);
        }

        //TODO make into info?
        public Task<CreateGroupCompetitionResult> CreateGroupCompetition(string title, MetricType metric, DateTime startDate, DateTime endDate) {
            var request = new CreateCompetitionRequest(){Title = title, Metric = metric, EndTime = endDate, StartTime = startDate, GroupId = _configuration.GroupId, VerificationCode = _configuration.GroupVerificationCode};
            return _highscoreApiRepository.CreateGroupCompetition(request);
        }

        public Task<bool> CreateGuildCompetition(IGuildUser user, int id) {
            if (_repository.GetAllActiveGuildCompetitions(user.GuildId).Any()) {
                throw new Exception($"There is already a competition running for this guild. Please clear this competition first.");
            }

            GuildCompetition comp = new GuildCompetition(user, id);
            _repository.InsertGuildCompetition(comp);
            return Task.FromResult(true);
        }

        public Task<bool> DeleteGuildCompetition(int id, DateTime endDate) {
            GuildCompetition comp = _repository.GetGuildCompetitionById(id);
            comp.EndTime = endDate;
            _repository.UpdateGuildCompetition(comp);
            return Task.FromResult(true);
        }
    }
}