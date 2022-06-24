using DiscordBot.Common.Models.Enums;
using DiscordBot.Services.Models.Enums;
using MathieuDR.Common.Parsers;

namespace DiscordBot.Commands.Interactive;

public class CreateCompetitionCommandHandler : ApplicationCommandHandler {
    private const string StartDateOption = "start";
    private const string EndDateOption = "end";
    private const string MetricOption = "metric";
    private const string NameOption = "name";
    private const string TeamOption = "competition-type";
    private readonly IGroupService _groupService;
    private readonly MetricTypeParser _metricTypeParser;

    public CreateCompetitionCommandHandler(ILogger<CreateCompetitionCommandHandler> logger, MetricTypeParser metricTypeParser,
        IGroupService groupService)
        : base("competition", "Create a WOM competition", logger) {
        _metricTypeParser = metricTypeParser;
        _groupService = groupService;
    }

    public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanModerator;

    public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
        _ = context.DeferAsync(ephemeral: true);

        var startString = context.Options.GetOptionValue<string>(StartDateOption);
        var endString = context.Options.GetOptionValue<string>(EndDateOption);
        var metricString = context.Options.GetOptionValue<string>(MetricOption);
        var nameString = context.Options.GetOptionValue<string>(NameOption);
        var compType = context.Options.ContainsKey(TeamOption) ? (CompetitionType)context.Options.GetOptionValue<long>(TeamOption) : CompetitionType.Normal;

        var start = startString.ToFutureDate();
        var end = endString.ToFutureDate();
        var canParse = _metricTypeParser.TryParseToMetricType(metricString, out var metric);

        if (start.IsFailed) {
            return Result.Merge(Result.Fail("Please enter a correct start date"), start).ToResult();
        }

        if (end.IsFailed) {
            return Result.Merge(Result.Fail("Please enter a correct end date"), end).ToResult();
        }

        if (start.Value >= end.Value) {
            return Result.Fail("Start date needs to be before end date");
        }

        if (!canParse) {
            return Result.Fail("I do not recognize this metric!");
        }
     
        var createRequest = await _groupService.CreateCompetition(context.Guild.ToGuildDto(),
            start.Value, end.Value, metric, compType, nameString);

        if (createRequest.IsFailed) {
            return createRequest.ToResult();
        }

        await context
            .CreateReplyBuilder(true)
            .WithEmbed(builder =>
                builder.AddWiseOldMan(createRequest.Value))
            .WithEphemeral()
            .FollowupAsync();

        return Result.Ok();
    }

    protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
        builder
            .AddOption(StartDateOption, ApplicationCommandOptionType.String, "The start date and time", true)
            .AddOption(EndDateOption, ApplicationCommandOptionType.String, "The end date and time", true)
            .AddOption(MetricOption, ApplicationCommandOptionType.String, "The metric of the competition", true)
            .AddEnumOption<CompetitionType>(TeamOption, "Select competition type", false)
            .AddOption(NameOption, ApplicationCommandOptionType.String, "A custom name for the competition", false);

        return Task.FromResult(builder);
    }
}
