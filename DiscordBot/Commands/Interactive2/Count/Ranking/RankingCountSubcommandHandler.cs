using System.Text;
using DiscordBot.Common.Models.Data.Counting;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Count.Ranking;

public class RankingCountSubcommandHandler : ApplicationCommandHandlerBase<RankingCountSubCommandRequest> {
    private readonly ICounterService _countService;
    private const int Quantity = 30;

    public RankingCountSubcommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _countService = serviceProvider.GetRequiredService<ICounterService>();
    }
    protected override Task<Result> DoWork(CancellationToken cancellationToken) {
        if (!Context.InGuild) {
            return Task.FromResult(Result.Fail("Command needs to be executed in a guild."));
        }

        var user = GetUser();
        List<UserCountInfo> rankingMembers;
        var startIndex = 0;
        
        if (user is null) {
            rankingMembers = _countService.TopCounts(Context.Guild.ToGuildDto(), Quantity);
        } else {
            (rankingMembers, startIndex) = _countService.CountRanking(user.ToGuildUserDto(), Quantity);
        }

        var embed = CreateEmbed(rankingMembers, startIndex);
        _ = Context
            .CreateReplyBuilder()
            .WithEmbed(embed)
            .RespondAsync();
        
        return Task.FromResult(Result.Ok());
    }

    private EmbedBuilder CreateEmbed(List<UserCountInfo> rankingMembers, int startIndex) {
        var builder = Context.CreateEmbedBuilder("Leaderboards");

        if (rankingMembers.Count == 0) {
            return builder.WithDescription("No user has any points.");
        }

        var countAdd = 1 + startIndex;
        var descriptionBuilder = new StringBuilder();
        for (var i = 0; i < rankingMembers.Count; i++) {
            var countInfo = rankingMembers[i];
            descriptionBuilder.Append($"{(i + countAdd).ToString()}, ".PadLeft(4));
            descriptionBuilder.Append($"{Context.GetDisplayNameById(countInfo.DiscordId)}".PadRight(25));
            descriptionBuilder.AppendLine($": {countInfo.CurrentCount.ToString()} points".PadRight(13));
        }

        builder.WithDescription($"```{descriptionBuilder}```");
        return builder;
    }

    private IUser? GetUser() {
        return Context.SubCommandOptions.GetOptionValue<IUser>(RankingCountSubcommandDefinition.UserOption);
    }
}