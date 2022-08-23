using System.Text;
using DiscordBot.Common.Models.Data.Counting;
using Fergun.Interactive.Pagination;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Count.Log;

public class LogCountSubCommandHandler : ApplicationCommandHandlerBase<LogCountSubCommandRequest> {
    private readonly ICounterService _countService;

    public LogCountSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _countService = serviceProvider.GetRequiredService<ICounterService>();
    }
    protected override Task<Result> DoWork(CancellationToken cancellationToken) {
        if (!Context.InGuild) {
            return Task.FromResult(Result.Fail("Command needs to be executed in a guild."));
        }

        var user = GetUser();
        var countInfo = _countService.GetCountInfo(user.ToGuildUserDto());
        
        var historyPagesInformation = CountHistoryToString(countInfo);
        var paginator = CreatePaginator(historyPagesInformation, user, countInfo);
        _ = Context.SendPaginator(paginator.Build(), cancellationToken: cancellationToken);
        return Task.FromResult(Result.Ok());
    }

    private StaticPaginatorBuilder CreatePaginator(List<string> historyPagesInformation, IUser user, UserCountInfo countInfo) {
        var pages = new List<PageBuilder>();
        foreach (var page in historyPagesInformation) {
            var descriptionBuilder = new StringBuilder();
            descriptionBuilder.AppendLine("```diff");
            descriptionBuilder.Append(page);
            descriptionBuilder.AppendLine("```");

            var builder = Context.CreatePageBuilder(descriptionBuilder.ToString())
                .WithTitle($"Total count for {user.DisplayName()}: {countInfo.CurrentCount}.");

            pages.Add(builder);
        }

        var paginator = Context.GetBaseStaticPaginatorBuilder(pages);
        return paginator;
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

    private IUser GetUser() {
        return Context.SubCommandOptions.GetOptionValue<IUser>(LogCountSubCommandDefinition.UserOption) ?? Context.GuildUser;
    }
}