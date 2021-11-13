using System;
using System.Threading.Tasks;
using Common.Parsers;
using Discord;
using DiscordBot.Helpers.Builders;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models.Contexts;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Models.Enums;
using DiscordBot.Transformers;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive {
    public class CreateCompetitionCommandHandler : ApplicationCommandHandler {
        private readonly MetricTypeParser _metricTypeParser;
        private readonly IGroupService _groupService;

        public CreateCompetitionCommandHandler(ILogger<CreateCompetitionCommandHandler> logger, MetricTypeParser metricTypeParser, IGroupService groupService) 
            : base("competition", "Create a WOM competition", logger) {
            _metricTypeParser = metricTypeParser;
            _groupService = groupService;
        }
        public override Guid Id => Guid.Parse("B6D60A7A-68F5-42AB-8745-269D575EEFE4");
  
        public override async Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            _ = context.DeferAsync();
            
            var startString = context.Options.GetOptionValue<string>(StartDateOption);
            var endString = context.Options.GetOptionValue<string>(EndDateOption);
            var metricString = context.Options.GetOptionValue<string>(MetricOption);
            var nameString = context.Options.GetOptionValue<string>(NameOption);
            var compType = (CompetitionType)context.Options.GetOptionValue<long>(TeamOption);

            var start = startString.ToFutureDate();
            var end = endString.ToFutureDate();
            var canParse = _metricTypeParser.TryParseToMetricType(metricString, out MetricType metric);

            if (start.IsFailed) {
                return Result.Merge(Result.Fail("Please enter a correct start date"), start);
            }
            
            if (end.IsFailed) {
                return Result.Merge(Result.Fail("Please enter a correct end date"), end);
            }

            if (start.Value >= end.Value) {
                return Result.Fail("Start date needs to be before end date");
            }

            if (!canParse) {
                return Result.Fail("I do not recognize this metric!");
            }

            var createRequest = await _groupService.CreateCompetition(context.Guild.ToGuildDto(),
                new DateTimeOffset(start.Value),
                new DateTimeOffset(end.Value), metric, compType, nameString);

            if (createRequest.IsFailed) {
                return createRequest;
            }

            await context
                .CreateReplyBuilder(true)
                .WithEmbed(builder => 
                    builder.AddWiseOldMan(createRequest.Value))
                .FollowupAsync();

            return Result.Ok();
        }

        public override Task<Result> HandleComponentAsync(MessageComponentContext context) {
            throw new NotImplementedException();
        }
        
        private const string StartDateOption = "start";
        private const string EndDateOption = "end";
        private const string MetricOption = "metric";
        private const string NameOption = "name";
        private const string TeamOption = "competition-type";

        protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
            builder
                .AddOption(StartDateOption, ApplicationCommandOptionType.String, "The start date and time", true)
                .AddOption(EndDateOption, ApplicationCommandOptionType.String, "The end date and time", true)
                .AddOption(MetricOption, ApplicationCommandOptionType.String, "The metric of the competition", true)
                .AddEnumOption<CompetitionType>(TeamOption, "Select competition type", true)
                .AddOption(NameOption, ApplicationCommandOptionType.String, "A custom name for the competition", false);

            return Task.FromResult(builder);
        }
    }
}
