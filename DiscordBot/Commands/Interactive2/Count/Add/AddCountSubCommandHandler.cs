using System.Text;
using DiscordBot.Common.Dtos.Discord;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Count.Add;

public class AddCountSubCommandHandler : ApplicationCommandHandlerBase<AddCountSubCommandRequest> {
    private readonly ICounterService _countService;

    public AddCountSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
        _countService = serviceProvider.GetRequiredService<ICounterService>();
    }
    protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
        await Context.DeferAsync();
        var (score, users, reason) = await GetOptions();

        var r = ValidateOptions(score, users);
        if (r.IsFailed) {
            return r;
        }

        var scoreDict =  await ApplyScores(users, score, reason);
        return SendEmbed(score, reason, scoreDict);
    }

    private Task<Dictionary<GuildUser, int>> ApplyScores(List<IUser> users, int score, string reason) {
        return _countService.Count(users.Select(x=>x.ToGuildUserDto()), Context.GuildUser.ToGuildUserDto(), score, reason);
    }

    private Result SendEmbed(int score, string reason, Dictionary<GuildUser, int> dataDict) {
        var title = $"Changed {dataDict.Count} users";
        var descr = BuildDescription(score, reason, dataDict);

        _ = Context.CreateReplyBuilder()
            .WithEmbed(b=>
                b.WithSuccess(descr, title))
            .FollowupAsync();
        
        return Result.Ok();
    }

    private static string BuildDescription(int score, string reason, Dictionary<GuildUser, int> dataDict) {
        var descrBuilder = new StringBuilder();
        descrBuilder.Append("Added ");
        descrBuilder.Append(score);
        descrBuilder.Append(" points for '");
        descrBuilder.Append(reason);
        descrBuilder.AppendLine("'");
        descrBuilder.AppendLine("");

        foreach (var kvp in dataDict) {
            descrBuilder.Append(kvp.Key.Username);
            descrBuilder.Append(" point total: ");
            descrBuilder.AppendLine(kvp.Value.ToString());
        }

        return descrBuilder.ToString();
    }

    private Result ValidateOptions(long score, List<IUser> users) {
        if (!Context.InGuild) {
            return Result.Fail("Command need to be executed in a guild");
        }
        
        if (score == 0) {
            return Result.Fail($"The score cannot be 0.{Environment.NewLine}Please use a negative or positive score.");
        }

        if (!users.Any()) {
            return Result.Fail($"There are no users to give a score.");
        }

        return Result.Ok();
    }

    private async Task<(int score, List<IUser> users, string reason)> GetOptions() {
        var value = Context.SubCommandOptions.GetOptionValue<long>(AddCountSubCommandDefinition.ValueOption);
        var usersString = Context.SubCommandOptions.GetOptionValue<string>(AddCountSubCommandDefinition.UsersOption);
        var reason = Context.SubCommandOptions.GetOptionValue<string>(AddCountSubCommandDefinition.ReasonOption);
        var users = (await usersString.GetUsersFromString(Context)).users;

        return ((int)value, users.ToList(), reason);
    }
}
