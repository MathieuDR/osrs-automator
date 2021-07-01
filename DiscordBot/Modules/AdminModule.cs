using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Helpers;
using DiscordBot.Models.Configuration;
using DiscordBot.Models.Enums;
using DiscordBot.Preconditions;
using DiscordBot.Services.interfaces;

namespace DiscordBot.Modules {
    [Name("Administrator")]
    [Group("cfg")]
    [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
    [RequireRole(new ulong[]{784510650260914216, 806544893584343092}, Group = "Permission")]
    public class AdminModule : BaseWaitMessageEmbeddedResponseModule {
        private readonly IGroupService _groupService;


        public AdminModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration,
            IGroupService groupService) :
            base(mapper, logger, messageConfiguration) {
            _groupService = groupService;
        }
        
        [Name("DisplayMembers")]
        [Command("members", RunMode = RunMode.Async)]
        [Summary("Display all members")]
        [RequireContext(ContextType.Guild)]
        public async Task Members() {
            var members = Context.Guild.Users;
            var builder = new EmbedBuilder().AddCommonProperties().WithMessageAuthorFooter(Context);
            builder.Title = $"Success - {members.Count}";
            builder.Description = string.Join(", ", members.Select(x => x.Username).ToList());
            await ModifyWaitMessageAsync(builder.Build());
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
                .WithMessageAuthorFooter(Context)
                .WithTitle("Success!")
                .WithDescription($"Channel {messageChannel.Name} set for job '{jobType}'");

            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Queue job")]
        [Command("queue")]
        [Summary("Queue automated job")]
        [RequireContext(ContextType.Guild)]
        public async Task QueueAutomated(string job) {
            var jobType = Enum.Parse<JobType>(job, true);
            _ = _groupService.QueueJob(jobType);
                
            var builder = new EmbedBuilder()
                .AddCommonProperties()
                .WithMessageAuthorFooter(Context)
                .WithTitle("Success!")
                .WithDescription($"Job '{jobType}' queued");

            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Toggle job")]
        [Command("toggle automated")]
        [Summary("Toggle job")]
        [RequireContext(ContextType.Guild)]
        public async Task ToggleAutomatedJob(string job) {
            var jobType = Enum.Parse<JobType>(job, true);
            bool activated = await _groupService.ToggleAutomationJob(jobType, GetGuildUser().Guild);

            string verb = activated ? "activated" : "deactivated";

            var builder = new EmbedBuilder()
                .AddCommonProperties()
                .WithMessageAuthorFooter(Context)
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
                .WithMessageAuthorFooter(Context)
                .WithTitle("Settings")
                .AddFieldsFromDictionary(settings);

            await ModifyWaitMessageAsync(builder.Build());
        }
    }
}