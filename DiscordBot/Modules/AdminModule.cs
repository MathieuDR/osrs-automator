using System;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {

    [Name("Administrator")]
    [Group("cfg")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : BaseWaitMessageEmbeddedResponseModule {
        private readonly IAuthenticationService _authenticationService;
        private readonly IGroupService _groupService;


        public AdminModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration, IAuthenticationService authenticationService, IGroupService groupService) : base(mapper, logger, messageConfiguration) {
            _authenticationService = authenticationService;
            _groupService = groupService;
        }

        [Name("Toggle Permission on Role")]
        [Command("right")]
        [Summary("Toggle a permission for a role.")]
        public Task ToggleRolePermission(IRole role, BotPermissions permission) {
            throw new NotImplementedException();
        }

        [Name("Toggle Permission on User")]
        [Command("right")]
        [Summary("Toggle a permission for an user.")]
        public Task ToggleUserPermission(IGuildUser user, BotPermissions permission) {
            throw new NotImplementedException();
        }

        [Name("Set WOM group")]
        [Command("womgroup", RunMode = RunMode.Async)]
        [Summary("Set the wom group for guild")]
        [RequireContext(ContextType.Guild)]
        public async Task SetWomGroup(int womGroup, string verificationCode) {
            var group = await _groupService.SetGroupForGuild(GetGuildUser(), womGroup, verificationCode);
            var builder = Context.CreateCommonWiseOldManEmbedBuilder();
            builder.Title = $"Success.";
            builder.Description = $"Group set to {group.Name}";
            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Auto add group")]
        [Command("autoadd", RunMode = RunMode.Async)]
        [Summary("Auto add group")]
        [RequireContext(ContextType.Guild)]
        public async Task SetAutoAdd(bool autoAdd) {
            await _groupService.SetAutoAdd(GetGuildUser(), autoAdd);
            var builder = Context.CreateCommonWiseOldManEmbedBuilder();
            builder.Title = $"Success.";
            if (autoAdd) {
                builder.Description = $"New members will be automatically added.";
            } else {
                builder.Description = $"New members will not be automatically added.";
            }
            
            await ModifyWaitMessageAsync(builder.Build());
        }
    }
}