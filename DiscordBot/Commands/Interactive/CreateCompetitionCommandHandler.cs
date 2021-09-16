using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Models;
using DiscordBot.Models.Contexts;
using DiscordBot.Models.Enums;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive {
    public class CreateCompetitionCommandHandler : ApplicationCommandHandler {
        public CreateCompetitionCommandHandler(ILogger<CreateCompetitionCommandHandler> logger) 
            : base("competition", "Create a WOM competition", logger) { }
        public override Guid Id => Guid.Parse("B6D60A7A-68F5-42AB-8745-269D575EEFE4");

        private readonly Dictionary<ulong, CreateCompetition> _createCompetitionDictionary =
            new();
        public async override Task<Result> HandleCommandAsync(ApplicationCommandContext context) {
            var startString = context.Options.GetOptionValue<string>(StartDateOption);
            var endString = context.Options.GetOptionValue<string>(EndDateOption);
            var category = context.Options.GetOptionValue<MetricTypeCategory>(MetricOption);
            var compType = context.Options.GetOptionValue<CompetitionType>(TeamOption);
            
            var model = new CreateCompetition();
            
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
