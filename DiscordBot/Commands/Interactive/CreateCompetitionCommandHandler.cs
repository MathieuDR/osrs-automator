using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Parsers;
using Discord;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models;
using DiscordBot.Models.Contexts;
using DiscordBot.Models.Enums;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive {
    public class CreateCompetitionCommandHandler : ApplicationCommandHandler {
        private readonly MetricTypeParser _metricTypeParser;

        public CreateCompetitionCommandHandler(ILogger<CreateCompetitionCommandHandler> logger, MetricTypeParser metricTypeParser) 
            : base("competition", "Create a WOM competition", logger) {
            _metricTypeParser = metricTypeParser;
        }
        public override Guid Id => Guid.Parse("B6D60A7A-68F5-42AB-8745-269D575EEFE4");

        private readonly Dictionary<ulong, CreateCompetition> _createCompetitionDictionary =
            new();
        public async override Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            var startString = context.Options.GetOptionValue<string>(StartDateOption);
            var endString = context.Options.GetOptionValue<string>(EndDateOption);
            var metricString = context.Options.GetOptionValue<string>(MetricOption);
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

            await context
                .CreateReplyBuilder(true)
                .WithEmbedFrom("Success",
                    $"Creating a competition from {start.Value:g} to {end.Value:g} in {metric.GetEnumValueNameOrDefault()} with type {compType.GetEnumValueNameOrDefault()}")
                .RespondAsync();

            return Result.Ok();
        }

        public override Task<Result> HandleComponentAsync(MessageComponentContext context) {
            throw new NotImplementedException();
        }
        
        private const string StartDateOption = "start";
        private const string EndDateOption = "end";
        private const string MetricOption = "metric";
        private const string TeamOption = "competition-type";

        protected override Task<SlashCommandBuilder> ExtendSlashCommandBuilder(SlashCommandBuilder builder) {
            builder
                .AddOption(StartDateOption, ApplicationCommandOptionType.String, "The start date and time")
                .AddOption(EndDateOption, ApplicationCommandOptionType.String, "The end date and time")
                .AddOption(MetricOption, ApplicationCommandOptionType.String, "The metric of the competition")
                .AddEnumOption<CompetitionType>(TeamOption, "Select competition type");

            return Task.FromResult(builder);
        }
    }
}
