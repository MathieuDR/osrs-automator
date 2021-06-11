using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    
    [Group("count")]
    public class CountModule: BaseWaitMessageEmbeddedResponseModule  {
        private readonly ICounterService _counterService;

        public CountModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration, ICounterService counterService) : base(mapper,
            logger, messageConfiguration) {
            _counterService = counterService;
        }

        [Name("Count")]
        [Command]
        [Summary("Add any number to the tally")]
        [RequireContext(ContextType.Guild)]
        public async Task Count(int additive, IUser user, [Remainder]string reason = null) {
            if (additive == 0) {
                throw new ArgumentException($"Additive must not be 0");
            }

            var guildUser = user as IGuildUser ?? throw new ArgumentException("Cannot find user");
            var totalCount = _counterService.Count(guildUser, (IGuildUser)Context.User, additive,reason);

            var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder()).WithTitle($"New count for {guildUser.Nickname}")
                .WithDescription($"Total count: {totalCount}").WithMessageAuthorFooter(Context);
            
            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Count")]
        [Command]
        [Summary("See total count")]
        [RequireContext(ContextType.Guild)]
        public async Task GetTotal(IGuildUser user) {
            var totalCount = _counterService.TotalCount(user);
        
            var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder()).WithTitle(user.Nickname)
                .WithDescription($"Total count: {totalCount}").WithMessageAuthorFooter(Context);
            
            await ModifyWaitMessageAsync(builder.Build());
        }
        
        [Name("Count history")]
        [Command("history")]
        [Summary("See count history")]
        [RequireContext(ContextType.Guild)]
        public async Task CountHistory(IGuildUser user) {
            var countInfo = _counterService.GetCountInfo(user);

            var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder()).WithTitle($"{user.Nickname} count history")
                .WithMessageAuthorFooter(Context);
            
            CountHistoryToDescription(builder, countInfo);
            
            await ModifyWaitMessageAsync(builder.Build());
        }
        
        [Name("Count history")]
        [Command("history")]
        [Summary("See count history")]
        [RequireContext(ContextType.Guild)]
        public async Task CountHistory() {
            await CountHistory((IGuildUser) Context.User);
        }
        
        [Name("Count history")]
        [Command("top")]
        [Summary("See count history")]
        [RequireContext(ContextType.Guild)]
        public async Task CountTop() {
            await CountTop(10);
        }
        
        [Name("Count history")]
        [Command("top")]
        [Summary("See count history")]
        [RequireContext(ContextType.Guild)]
        public async Task CountTop(int quantity) {
            if (quantity > 20) {
                throw new ArgumentException("Not more then 20 members");
            }

            var topMembers = _counterService.TopCounts(Context.Guild, quantity);
            var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder())
                .WithTitle($"Top counts for {Context.Guild.Name}")
                .WithMessageAuthorFooter(Context);
            
            ListTopMembers(builder, topMembers);
            
            await ModifyWaitMessageAsync(builder.Build());
        }

        private void CountHistoryToDescription(EmbedBuilder builder, UserCountInfo countInfo) {
            var historyBlockBuilder = new StringBuilder();
            foreach (var count in countInfo.CountHistory) {
                historyBlockBuilder.Append(count.Additive > 0 ? "+ " : "- ");
                historyBlockBuilder.Append($"{Math.Abs(count.Additive)}".PadLeft(4));
                historyBlockBuilder.Append(string.IsNullOrEmpty(count.Reason)? "".PadRight(25): $", {count.Reason}".PadRight(25));
                historyBlockBuilder.AppendLine(
                    $"| By {count.RequestedDiscordTag} on {count.RequestedOn.ToString("d")}");
            }

            builder.WithDescription($"Total count: {countInfo.CurrentCount}.{Environment.NewLine}```diff{Environment.NewLine}{historyBlockBuilder}```");
        }
        
        private void ListTopMembers(EmbedBuilder builder, List<UserCountInfo> countInfos) {
            var historyBlockBuilder = new StringBuilder();
            for (var i = 0; i < countInfos.Count; i++) {
                var countInfo = countInfos[i];
                historyBlockBuilder.Append($"{i+1}, ".PadLeft(4));
                historyBlockBuilder.Append($"{NicknameById(countInfo.DiscordId)}".PadRight(25));
                historyBlockBuilder.AppendLine(
                    $": {countInfo.CurrentCount} points".PadRight(13));
            }

            builder.WithDescription($"```{historyBlockBuilder}```");
        }

        private string NicknameById(ulong userId) {
            var user = Context.Guild.GetUser(userId);
            return user?.Nickname ?? "Unknown user";
        }
        
        [Group("treshold")]
        public class CountConfigModule : BaseWaitMessageEmbeddedResponseModule{
            private readonly IGroupService _groupService;
            public CountConfigModule(IGroupService groupService,Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration) : base(mapper, logger, messageConfiguration) {
                _groupService = groupService;
            }

            public Task Set(int count, IRole role, [Remainder] string message) {
                
            }
            
            public Task List() {
                
            }
            
            public Task Remove(string id) {
                
            }
        }
    }
}
