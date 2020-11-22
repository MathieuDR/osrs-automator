using System;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
            var decoratedGroup = await _groupService.SetGroupForGuild(GetGuildUser(), womGroup, verificationCode);
            var builder = new EmbedBuilder().AddWiseOldMan(decoratedGroup);
                
            builder.Title = $"Success.";
            builder.Description = $"Group set to {decoratedGroup.Item.Name}";
            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Auto add group")]
        [Command("autoadd", RunMode = RunMode.Async)]
        [Summary("Auto add group")]
        [RequireContext(ContextType.Guild)]
        public async Task SetAutoAdd(bool autoAdd) {
            await _groupService.SetAutoAdd(GetGuildUser(), autoAdd);
            var builder = new EmbedBuilder().AddCommonProperties().AddFooterFromMessageAuthor(Context);
            
            builder.Title = $"Success.";
            builder.Description = autoAdd ? $"New members will be automatically added." : $"New members will not be automatically added.";
            
            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Set Automated message channel")]
        [Command("set automated")]
        [Summary("Set Automated message channel")]
        [RequireContext(ContextType.Guild)]
        public async Task SetAutoChannel(string job, IChannel channel) {
            //var channel = Context.Channel;
            var messageChannel = (ISocketMessageChannel) channel;

            if (messageChannel == null) {
                throw new Exception($"Channel wasn't a message channel. Try a different one.");
            }

            var jobType = Enum.Parse<JobType>(job, true);

            await _groupService.SetAutomationJobChannel(jobType, GetGuildUser(), messageChannel);

            var builder = new EmbedBuilder()
                .AddCommonProperties()
                .AddFooterFromMessageAuthor(Context)
                .WithTitle("Success!")
                .WithDescription($"Channel {messageChannel.Name} set for job '{jobType}'");
            
            await ModifyWaitMessageAsync(builder.Build());
        }

        
        [Name("Set Automated message channel")]
        [Command("toggle automated")]
        [Summary("Set Automated message channel")]
        [RequireContext(ContextType.Guild)]
        public async Task ToggleAutomatedJob(string job) {
            var jobType = Enum.Parse<JobType>(job, true);
            
            bool activated = await _groupService.ToggleAutomationJob(jobType, GetGuildUser().Guild);

            string verb = activated  ? "activated" : "deactivated";

            var builder =new EmbedBuilder()
                .AddCommonProperties()
                .AddFooterFromMessageAuthor(Context)
                .WithTitle("Success!")
                .WithDescription($"Job '{jobType}' {verb}");
            
            await ModifyWaitMessageAsync(builder.Build());

        }

        [Name("Read config")]
        [Command("view", RunMode = RunMode.Async)]
        [Summary("Views the current configuration settings")]
        [RequireContext(ContextType.Guild)]
        public async Task ViewSettings() {
            var settings = await _groupService.GetSettingsDictionary(GetGuildUser().Guild);

            var builder = new EmbedBuilder()
                .AddCommonProperties()
                .AddFooterFromMessageAuthor(Context)
                .WithTitle("Settings")
                .AddFieldsFromDictionary(settings);
            
            await ModifyWaitMessageAsync(builder.Build());
        }
    }
}