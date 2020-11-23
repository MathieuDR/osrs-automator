using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.ResponseModels;
using DiscordBotFanatic.Paginator;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    public class CompetitionModule : BaseWaitMessageEmbeddedResponseModule {
        private readonly ICompetitionService _competitionService;

        public CompetitionModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration,
            ICompetitionService competitionService) : base(mapper, logger, messageConfiguration) {
            _competitionService = competitionService;
        }


        [Name("Set WOM Competition")]
        [Command("comp browse", RunMode = RunMode.Async)]
        [Summary("Set the current competition")]
        [RequireContext(ContextType.Guild)]
        public async Task BrowseCompetitions() {
            // Can perhaps 'select' to see more details of a competition?
            var competitionWrappers = (await _competitionService.ViewCompetitionsForGroup(GetGuildUser())).ToList();

            var builder = new EmbedBuilder()
                .AddWiseOldMan(competitionWrappers.FirstOrDefault())
                .WithTitle($"Current running competitions.")
                .AddFooterFromMessageAuthor(Context);
            
            var message = new CustomPaginatedMessage(builder) {
                Pages = competitionWrappers.Select(wrapper => wrapper.Item).ToPaginatedStringWithContexts(),
                Options = new CustomActionsPaginatedAppearanceOptions()
            };

            await SendPaginatedMessageAsync(message);
        }

        //private async Task SetCompetition(IGuildUser user, Competition competition) {
        //    var compFromService = await _competitionService.SetCurrentCompetition(user, competition.Id);

        //    var builder = Context.CreateCommonEmbedBuilder();
        //    builder.Title = $"Success! Current competition set to";
        //    builder.Description = compFromService.ToCompetitionInfoString();

        //    await ReplyAsync(null,  false, builder.Build());
        //}

        [Name("View the metadata of the current competition")]
        [Command("comp view", RunMode = RunMode.Async)]
        [Summary("View the current competition")]
        [RequireContext(ContextType.Guild)]
        public async Task ViewCurrentCompetition() {
            var competitionDecorator = await _competitionService.ViewCurrentCompetition(GetGuildUser().Guild);
            var competition = competitionDecorator.Item;

            var builder = new EmbedBuilder()
                .AddWiseOldMan(competitionDecorator)
                .WithTitle("Current competition")
                .WithDescription($"The servers current competition is set to: {competitionDecorator.Title}")
                .AddFooterFromMessageAuthor(Context);


            builder.AddField("Start date", competition.StartDate);
            builder.AddField("End date", competition.EndDate);
            builder.AddField("Duration", competition.Duration);
            builder.AddField("ParticipantCount", competition.ParticipantCount);

            await ModifyWaitMessageAsync(builder.Build());
        }
    }
}