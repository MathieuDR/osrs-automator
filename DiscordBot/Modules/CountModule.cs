using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Helpers;
using DiscordBot.Models.Configuration;
using DiscordBot.Models.Data;
using DiscordBot.Models.ResponseModels;
using DiscordBot.Preconditions;
using DiscordBot.Services.interfaces;

namespace DiscordBot.Modules {
    
    [Group("count")]
    [RequireContext(ContextType.Guild)]
    public class CountModule: BaseWaitMessageEmbeddedResponseModule  {
        private readonly ICounterService _counterService;
        private readonly IServiceProvider _serviceProvider;

        public CountModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration, 
            ICounterService counterService, IServiceProvider serviceProvider) : base(mapper,
            logger, messageConfiguration) {
            _counterService = counterService;
            _serviceProvider = serviceProvider;
        }

        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireRole(new ulong[]{784510650260914216, 806544893584343092}, Group = "Permission")]
        [Name("Count")]
        [Command]
        [Summary("Add any number to the tally, you can tag multiple users")]
        public async Task Count(int additive, [Remainder]string args) {
            if (additive == 0) {
                throw new ArgumentException($"Additive must not be 0");
            }

            var users = args
                .ToCollectionOfParameters()
                .ToArray()
                .GetDiscordsUsersListFromStrings(out string[] remainingArgs, Context, _serviceProvider).Distinct()
                .ToList();
            
            if (!users.Any()) {
                throw new ArgumentException($"Must enter an user.");
            }

            var reason = string.Join(" ", remainingArgs.ParseMentions(Context));

            var userString = users.Count() == 1 ? "user" : "users";
            var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder()).WithTitle($"Adding {additive} points for {users.Count()} {userString}")
              .WithMessageAuthorFooter(Context);

            StringBuilder descriptionBuilder = new StringBuilder();
            var tasks = new List<Task>();
            foreach (var user in users) {
                var guildUser = user as IGuildUser ?? throw new ArgumentException("Cannot find user");
                var totalCount = _counterService.Count(guildUser, (IGuildUser)Context.User, additive, reason);

                var tresholdTask = HandleNewCount(totalCount - additive, totalCount, (IGuildUser) user);
                tasks.Add(tresholdTask);

                descriptionBuilder.AppendLine($"{guildUser.DisplayName()} new total: {totalCount}");
            }

            builder.WithDescription(descriptionBuilder.ToString());

            await ModifyWaitMessageAsync(builder.Build());
            await Task.WhenAll(tasks);
        }

        private async Task HandleNewCount(int startCount, int newCount, IGuildUser user) {
            try {
                var tresholds = await _counterService.GetTresholds(user.GuildId);
                var channelId = await _counterService.GetChannelForGuild(user.GuildId);

                if (!(Context.Guild.GetChannel(channelId) is ISocketMessageChannel channel)) {
                    return;
                }
                
                foreach (var treshold in tresholds) {
                    var tresholdCount = treshold.Threshold;

                    if (startCount < tresholdCount && newCount >= tresholdCount) {
                        // Hit it
                        await channel.SendMessageAsync($"{treshold.Name} hit for <@{user.Id}>!");
                        if (treshold.GivenRoleId.HasValue && Context.Guild.GetRole(treshold.GivenRoleId.Value) is IRole role) {
                            await user.AddRoleAsync(role);
                        }
                    }

                    if (startCount >= tresholdCount && newCount < tresholdCount) {
                        // Remove it
                        await channel.SendMessageAsync($"<@{user.Id}> has not sufficient points anymore for {treshold.Name}");
                        
                        if (treshold.GivenRoleId.HasValue && Context.Guild.GetRole(treshold.GivenRoleId.Value) is IRole role) {
                            await user.RemoveRoleAsync(role);
                        }
                    }
                }
            } catch (Exception e) {
                // ignored
            }
        }

        [Name("Total count")]
        [Command("total")]
        [Summary("See total count of an user")]
        public async Task GetTotal(IGuildUser user) {
            var totalCount = _counterService.TotalCount(user);
        
            var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder()).WithTitle(user.DisplayName())
                .WithDescription($"Total count: {totalCount}").WithMessageAuthorFooter(Context);
            
            await ModifyWaitMessageAsync(builder.Build());
        }
        
        [Name("Total count")]
        [Command("total")]
        [Summary("See your total count")]
        public async Task GetTotal() {
            IGuildUser user = (IGuildUser) Context.User;
            var totalCount = _counterService.TotalCount(user);
        
            var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder()).WithTitle(user.DisplayName())
                .WithDescription($"Total count: {totalCount}").WithMessageAuthorFooter(Context);
            
            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Count history")]
        [Command("history")]
        [Summary("See count history of an user")]
        public async Task CountHistory(IGuildUser user) {
            var countInfo = _counterService.GetCountInfo(user);

            var builder = new EmbedBuilder()
                .AddCommonProperties()
                .WithTitle($"{user.DisplayName()} count history")
                .WithMessageAuthorFooter(Context);

            if (countInfo is null || countInfo.CountHistory.Count == 0) {
                builder.WithDescription($"Has no history.");
                await ModifyWaitMessageAsync(builder.Build());
                return;
            }
            
            var historyPages = CountHistoryToString(countInfo);

            if (historyPages.Count == 1){
                builder.WithDescription($"Total count: {countInfo.CurrentCount}.{Environment.NewLine}```diff{Environment.NewLine}{historyPages.First()}```");
                await ModifyWaitMessageAsync(builder.Build());
                return;
            }
            
            var pages = historyPages.Select(x => builder.WithDescription($"Total count: {countInfo.CurrentCount}.{Environment.NewLine}```diff{Environment.NewLine}{x}```")).ToList();
            _ = DeleteWaitMessageAsync();
            
            // This needs to be refactored! ASAP
            var message = new CustomPaginatedMessage(new EmbedBuilder()) {
                Pages = pages
            };
            await SendPaginatedMessageAsync(message);
        }
        
        [Name("Count history")]
        [Command("history")]
        [Summary("See count history of yourself")]
        public async Task CountHistory() {
            await CountHistory((IGuildUser) Context.User);
        }
        
        [Name("Top")]
        [Command("top")]
        [Summary("See the top 10 users of the server")]
        public async Task CountTop() {
            await CountTop(10);
        }
        
        [Name("Top")]
        [Command("top")]
        [Summary("See the top users of the server. Maximum 20")]
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

        private List<string> CountHistoryToString( UserCountInfo countInfo) {
            var pages = new List<string>();
            int max = 1000;
            StringBuilder blockbuilder = new StringBuilder();

            var list = countInfo.CountHistory.OrderByDescending(x => x.RequestedOn).ToList();
            
            foreach (var count in list) {
                var historyBlockBuilder = new StringBuilder();
                historyBlockBuilder.Append(count.Additive > 0 ? "+ " : "- ");
                historyBlockBuilder.Append($"{Math.Abs(count.Additive)}".PadLeft(4));
                historyBlockBuilder.Append(string.IsNullOrEmpty(count.Reason)? "".PadRight(25): $", {count.Reason}".PadRight(25));
                historyBlockBuilder.Append($"| By {count.RequestedDiscordTag} on {count.RequestedOn.ToString("d")}");
                
                if (blockbuilder.ToString().Length + historyBlockBuilder.ToString().Length >= max) {
                    pages.Add(blockbuilder.ToString());
                    blockbuilder = new StringBuilder();
                }
                blockbuilder.AppendLine(historyBlockBuilder.ToString());
            }
            
            if(!string.IsNullOrWhiteSpace(blockbuilder.ToString())) {
                pages.Add(blockbuilder.ToString());
            }

            return pages;
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
            return user?.DisplayName();
        }
        
        [RequireUserPermission(GuildPermission.Administrator, Group = "Permission")]
        [RequireRole(new ulong[]{784510650260914216, 806544893584343092}, Group = "Permission")]
        [Group("treshold")]
        [RequireContext(ContextType.Guild)]
        public class CountConfigModule : BaseWaitMessageEmbeddedResponseModule{
            private readonly ICounterService _counterService;
            public CountConfigModule(ICounterService counterService,Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration) : base(mapper, logger, messageConfiguration) {
                _counterService = counterService;
            }

            [Name("Set channel for the outputs")]
            [Command("channel")]
            [Summary("Set channel for the outputs")]
            public async Task SetChannel(IChannel channel) {
                var success = await _counterService.SetChannelForCounts(Context.User as IGuildUser, channel);

                var verb = success ? "Success" : "Failure";
                var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder())
                    .WithTitle(verb)
                    .WithDescription($"Set {channel.Name} as output channel")
                    .WithMessageAuthorFooter(Context);

                await ModifyWaitMessageAsync(builder.Build());
            }

            [Name("Create")]
            [Command("create")]
            [Summary("Create a new treshold")]
            public async Task Set(int count, IRole role, [Remainder] string name) {
                var success = await _counterService.CreateTreshold(Context.User as IGuildUser, count, name, role);

                var verb = success ? "Success" : "Failure";
                var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder())
                    .WithTitle(verb)
                    .WithDescription($"Added new treshold '{name}' from {count} with role {role.Name}")
                    .WithColor(role.Color)
                    .WithMessageAuthorFooter(Context);

                await ModifyWaitMessageAsync(builder.Build());
            }
            
            [Name("Create")]
            [Command("create")]
            [Summary("Create a new treshold without a role")]
            public async Task SetWithoutRole(int count, [Remainder] string name) {
                var success = await _counterService.CreateTreshold(Context.User as IGuildUser, count, name);

                var verb = success ? "Success" : "Failure";
                var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder())
                    .WithTitle(verb)
                    .WithDescription($"Added new treshold '{name}' from {count} without role")
                    .WithMessageAuthorFooter(Context);

                await ModifyWaitMessageAsync(builder.Build());
            }
            
            [Name("List")]
            [Command("list")]
            [Summary("See all tresholds in a list format")]
            public async Task List() {
                var tresholds = await _counterService.GetTresholds(Context.Guild.Id);
                
                var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder())
                    .WithTitle("Current tresholds")
                    .WithDescription($"```{ToDescription(tresholds)}```")
                    .WithMessageAuthorFooter(Context);

                await ModifyWaitMessageAsync(builder.Build());
            }

            private string ToDescription(IEnumerable<CountThreshold> tresholds) {
                StringBuilder builder = new StringBuilder();
                var enumerable = tresholds.ToList();
                
                // format
                builder.Append("ID".PadLeft(3));
                builder.Append(", ");
                builder.Append($"#:".PadLeft(5));
                builder.Append("Name".PadRight(15));
                builder.Append($", role: role{Environment.NewLine}");
                
                for (var i = 0; i < enumerable.Count; i++) {
                    var treshold = enumerable[i];

                    builder.Append(i.ToString().PadLeft(3));
                    builder.Append(", ");

                    builder.Append($"{treshold.Threshold}:".PadLeft(5));
                        
                    var name = string.IsNullOrEmpty(treshold.Name) ? "Unnamed" : treshold.Name;
                    builder.Append(name.PadRight(15));


                    var role = "none";
                    if (treshold.GivenRoleId.HasValue) {
                        role = Context.Guild.GetRole(treshold.GivenRoleId.Value)?.Name ?? "Invalid role";
                    }

                    builder.Append($", role: {role}{Environment.NewLine}");
                }

                return builder.ToString();
            }
            
            [Name("Remove")]
            [Command("remove")]
            [Summary("Remove an treshold")]
            public async Task Remove(int index) {
                var success = await _counterService.RemoveCount(Context.Guild.Id, index);

                var verb = success ? "Success" : "Failure";
                var builder = EmbedBuilderHelper.AddCommonProperties(new EmbedBuilder())
                    .WithTitle(verb)
                    .WithDescription($"Removed treshold with index {index}")
                    .WithMessageAuthorFooter(Context);

                await ModifyWaitMessageAsync(builder.Build());
            }
        }
    }
}
