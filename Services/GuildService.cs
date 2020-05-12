using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
    public class GuildService : IGuildService{
        private readonly IDiscordBotRepository _repository;

        public GuildService(IDiscordBotRepository repository) {
            _repository = repository;
        }
        
        public bool HasActiveEvent(IGuild guild) {
            return _repository.GetAllActiveGuildEvents(guild.Id).Any();
        }
        public List<GuildEvent> GetActiveGuildEvents(IGuild guild) {
            return _repository.GetAllActiveGuildEvents(guild.Id).ToList();
        }

        public GuildEvent InsertGuildEvent(GuildEvent guildEvent) {
            guildEvent.IsValid();

            return _repository.InsertGuildEvent(guildEvent);
        }

        public bool EndGuildEvent(GuildEvent guildEvent) {
            throw new NotImplementedException();
        }

        public bool DoesUserHavePermission(IGuildUser user, Permissions permission) {
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

            if (arguments.Users.Count() >= guildEvent.MinimumPerCounter && arguments.Users.Count() <= guildEvent.MaximumPerCounter) {
                guildEvent.EventCounters??=new List<GuildEventCounter>();
                foreach (IUser user in arguments.Users) {
                    guildEvent.EventCounters.Add(new GuildEventCounter(){UserId = user.Id, ImageUrl = arguments.ImageUrl,});
                }

                return _repository.UpdateGuildEvent(guildEvent) != null;
            } else {
                throw new ArgumentException($"User count is wrong! Please use the `help` command and also check how many users the event require!");
            }
        }
    }
}