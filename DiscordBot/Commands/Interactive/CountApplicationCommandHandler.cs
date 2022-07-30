using System.Text;
using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive;

public class CountApplicationCommandHandler : ApplicationCommandHandler {
    private const string HistorySubCommandName = "history";
    private const string TopSubCommandName = "top";
    private const string AddSubCommandName = "add";
    private const string ValueOption = "value";
    private const string UserOption = "user";
    private const string ReasonOption = "reason";
    private const string UsersOption = "users";
    private readonly ICounterService _counterService;

    public CountApplicationCommandHandler(ILogger<CountApplicationCommandHandler> logger, ICounterService counterService) : base("count",
        "Everything about point counting!", logger) {
        _counterService = counterService;
    }

     public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanEventHost;


    public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
        var subCommand = context.Options.First().Key;

        var result = subCommand switch {
            HistorySubCommandName => await HistoryHandler(context),
            TopSubCommandName => await TopHandler(context),
            AddSubCommandName => await AddHandler(context),
            _ => Result.Fail("Did not find a correct handler")
        };

        return result;
    }

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        builder
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(HistorySubCommandName)
                .WithDescription("Check the count history of yourself or another user in the server")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(UserOption, ApplicationCommandOptionType.User, "The user that you want to see, leave blank for yourself", false)
            )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(AddSubCommandName)
                .WithDescription("Add a count to one or multiple users")
                .WithType(ApplicationCommandOptionType.SubCommand)
                .AddOption(ValueOption, ApplicationCommandOptionType.Integer, "The amount of points to count, can be negative to subtract", true)
                .AddOption(UsersOption, ApplicationCommandOptionType.String, "The users to be tagged", true)
                .AddOption(ReasonOption, ApplicationCommandOptionType.String, "The reason of the addition or subtraction", false)
            )
            .AddOption(new SlashCommandOptionBuilder()
                .WithName(TopSubCommandName)
                .WithDescription("Check the top counts of this server")
                .WithType(ApplicationCommandOptionType.SubCommand)
            );

        return Task.FromResult(builder);
    }

    #region add

    private async Task<Result> AddHandler(ApplicationCommandContext context) {
        await context.DeferAsync();
        var (additive, usersString, reason) = GetAddParameters(context);
        var users = (await usersString.GetUsersFromString(context)).users.ToList();

        if (additive == 0) {
            Result.Fail("Additive cannot be 0");
        }

        if (!users.Any()) {
            Result.Fail("No users found");
        }

        var descriptionBuilder = new StringBuilder();
        var tasks = HandleUsers(context, users, additive, reason, descriptionBuilder);
        var embed = CreateAddEmbed(context, users, additive, descriptionBuilder);

        await Task.WhenAll(tasks);
        await context.FollowupAsync(embeds: new[] { embed.Build() }, ephemeral: false);
        return Result.Ok();
    }

    private static EmbedBuilder CreateAddEmbed(ApplicationCommandContext context, List<IUser> users, int additive, StringBuilder descriptionBuilder) {
        var userString = users.Count() == 1 ? "user" : "users";
        var embed = context.CreateEmbedBuilder($"Adding {additive} points for {users.Count()} {userString}",
            descriptionBuilder.ToString());
        return embed;
    }

    private List<Task> HandleUsers(ApplicationCommandContext context, List<IUser> users, int additive, string reason,
        StringBuilder descriptionBuilder) {
        var tasks = new List<Task>();
        foreach (var user in users) {
            var guildUser = user as IGuildUser ?? throw new ArgumentException("Cannot find user");
            var totalCount = _counterService.Count(guildUser.ToGuildUserDto(), context.User.ToGuildUserDto(), additive, reason);

            var thresholdTask = HandleNewCount(context, totalCount - additive, totalCount, (IGuildUser)user);
            tasks.Add(thresholdTask);

            descriptionBuilder.AppendLine($"{guildUser.DisplayName()} new total: {totalCount}");
        }

        return tasks;
    }

    private static (int additive, string usersString, string reason) GetAddParameters(ApplicationCommandContext context) {
        var additive = (int)context.SubCommandOptions.GetOptionValue<long>(ValueOption);
        var usersString = context.SubCommandOptions.GetOptionValue<string>(UsersOption);
        var reason = context.SubCommandOptions.GetOptionValue<string>(ReasonOption);
        return (additive, usersString, reason);
    }

    private async Task HandleNewCount(ApplicationCommandContext context, int startCount, int newCount, IGuildUser user) {
        try {
            var thresholds = await _counterService.GetThresholds(user.GetGuildId()).OrderBy(x=>x.Threshold);;
            var channelId = await _counterService.GetChannelForGuild(user.GetGuildId());

            if (!(context.Guild.GetChannel(channelId.UlongValue) is ISocketMessageChannel channel)) {
                return;
            }

            foreach (var threshold in thresholds) {
                var thresholdThreshold = threshold.Threshold;

                if (startCount < thresholdThreshold && newCount >= thresholdThreshold) {
                    // Hit it
                    await channel.SendMessageAsync($"{threshold.Name} hit for <@{user.Id}>!");
                    if (threshold.GivenRoleId.HasValue && context.Guild.GetRole(threshold.GivenRoleId.Value.UlongValue) is IRole role) {
                        await user.AddRoleAsync(role);
                    }
                }

                if (startCount >= thresholdThreshold && newCount < thresholdThreshold) {
                    // Remove it
                    await channel.SendMessageAsync($"<@{user.Id}> has not sufficient points anymore for {threshold.Name}");

                    if (threshold.GivenRoleId.HasValue && context.Guild.GetRole(threshold.GivenRoleId.Value.UlongValue) is IRole role) {
                        await user.RemoveRoleAsync(role);
                    }
                }
            }
        } catch (Exception e) {
            // ignored
            Logger.LogError(e, "Could not handle new count");
        }
    }

    #endregion

    #region Top

    private Task<Result> TopHandler(ApplicationCommandContext context) {
        var topMembers = _counterService.TopCounts(context.Guild.ToGuildDto(), 20);

        var builder = context.CreateEmbedBuilder($"Top counts for {context.Guild.Name}");

        ListTopMembers(context, builder, topMembers);
        context.RespondAsync(embeds: new[] { builder.Build() }, ephemeral: false);
        return Task.FromResult(Result.Ok());
    }

    private void ListTopMembers(ApplicationCommandContext context, EmbedBuilder builder, List<UserCountInfo> countInfos) {
        var historyBlockBuilder = new StringBuilder();
        for (var i = 0; i < countInfos.Count; i++) {
            var countInfo = countInfos[i];
            historyBlockBuilder.Append($"{(i + 1).ToString()}, ".PadLeft(4));
            historyBlockBuilder.Append($"{context.GetDisplayNameById(countInfo.DiscordId)}".PadRight(25));
            historyBlockBuilder.AppendLine($": {countInfo.CurrentCount.ToString()} points".PadRight(13));
        }

        if (historyBlockBuilder.Length > 0) {
            builder.WithDescription($"```{historyBlockBuilder}```");
            return;
        }

        builder.WithDescription("Nobody has any points in this server");
    }

    #endregion

    #region history

    private async Task<Result> HistoryHandler(ApplicationCommandContext context) {
        var toSearchUser = context.SubCommandOptions.GetOptionValue<IGuildUser>(UserOption) ?? context.GuildUser;
        await CountHistory(context, toSearchUser);
        return Result.Ok();
    }

    private Task CountHistory(ApplicationCommandContext context, IGuildUser user) {
        var countInfo = _counterService.GetCountInfo(user.ToGuildUserDto());

        var historyPagesInformation = CountHistoryToString(countInfo);
        var pages = new List<PageBuilder>();
        foreach (var page in historyPagesInformation) {
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine("```diff");
            descriptionBuilder.Append(page);
            descriptionBuilder.AppendLine("```");

            var builder = context.CreatePageBuilder(descriptionBuilder.ToString())
                .WithTitle($"Total count for {user.DisplayName()}: {countInfo.CurrentCount}.");

            pages.Add(builder);
        }

        var paginator = context.GetBaseStaticPaginatorBuilder(pages);
        _ = context.SendPaginator(paginator.Build());
        return Task.CompletedTask;
    }

    private List<string> CountHistoryToString(UserCountInfo countInfo) {
        var pages = new List<string>();
        var max = 1000;
        var blockbuilder = new StringBuilder();

        var list = countInfo.CountHistory.OrderByDescending(x => x.RequestedOn).ToList();

        foreach (var count in list) {
            var historyBlockBuilder = new StringBuilder();
            historyBlockBuilder.Append(count.Additive > 0 ? "+ " : "- ");
            historyBlockBuilder.Append($"{Math.Abs(count.Additive).ToString()}".PadLeft(4));
            historyBlockBuilder.Append(string.IsNullOrEmpty(count.Reason) ? "".PadRight(25) : $", {count.Reason}".PadRight(25));
            historyBlockBuilder.Append($"| By {count.RequestedDiscordTag} on {count.RequestedOn.ToString("d")}");

            if (blockbuilder.ToString().Length + historyBlockBuilder.ToString().Length >= max) {
                pages.Add(blockbuilder.ToString());
                blockbuilder = new StringBuilder();
            }

            blockbuilder.AppendLine(historyBlockBuilder.ToString());
        }

        if (!string.IsNullOrWhiteSpace(blockbuilder.ToString())) {
            pages.Add(blockbuilder.ToString());
        }

        if (!pages.Any()) {
            pages.Add("No history");
        }

        return pages;
    }

    #endregion
}
