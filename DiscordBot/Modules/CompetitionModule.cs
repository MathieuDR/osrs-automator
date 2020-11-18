using System.Threading.Tasks;
using AutoMapper;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.ResponseModels;
using DiscordBotFanatic.Paginator;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {

    public class CompetitionModule : BaseWaitMessageEmbeddedResponseModule {
        private readonly ICompetitionService _competitionService;

        public CompetitionModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration, ICompetitionService competitionService) : base(mapper, logger, messageConfiguration) {
            _competitionService = competitionService;
        }


        [Name("Set WOM Competition")]
        [Command("comp browse", RunMode = RunMode.Async)]
        [Summary("Set the current competition")]
        [RequireContext(ContextType.Guild)]
        public async Task BrowseCompetitions() {
            // Can perhaps 'select' to see more details of a competition?
            var competitions = await _competitionService.ViewCompetitionsForGroup(GetGuildUser());
            
            var builder = Context.CreateCommonWiseOldManEmbedBuilder();
            builder.Title = $"Current running competitions.";
            //builder.Description = $"Select a competition using the right emoji!";

           var message = new CustomPaginatedMessage(builder) {
               Pages = competitions.ToPaginatedStringWithContexts(), 
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
            var competition = await _competitionService.ViewCurrentCompetition(GetGuildUser().Guild);
            
            var builder = Context.CreateCommonWiseOldManEmbedBuilder();
            builder.Title = $"Current competition";
            builder.Description = $"The servers current competition is set to: {competition.Title}";

            builder.AddField("Start date", competition.StartDate);
            builder.AddField("End date", competition.EndDate);
            builder.AddField("Duration", competition.Duration);
            builder.AddField("Participants", competition.Participants);

            await ModifyWaitMessageAsync(builder.Build());
        }

    }
}