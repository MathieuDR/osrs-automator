using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Models.ResponseModels;
using DiscordBotFanatic.Paginator;
using DiscordBotFanatic.Services.interfaces;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Modules {
    [Name("Player module")]
    public class PlayerModule : BaseWaitMessageEmbeddedResponseModule {
        private readonly IPlayerService _playerService;

        public PlayerModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration,
            IPlayerService playerService) : base(mapper, logger, messageConfiguration) {
            _playerService = playerService;
        }

        //[Name("Set Default OSRS username")]
        //[Command("setosrs", RunMode = RunMode.Async)]
        //[Summary("Set your default OSRS name for commands, and competitions")]
        //public Task SetOsrsName(string name) {
        //    throw new NotImplementedException();
        //    //var player = await _playerService.CoupleDiscordGuildUserToOsrsAccount(GetGuildUser(), name);
        //    //var b = Context.CreateCommonWiseOldManEmbedBuilder();
        //    //b.Description = $"Successfully set the default player to {player.DisplayName}";
        //    //await ModifyWaitMessageAsync(b.Build());
        //}

        [Name("Add an OSRS account")]
        [Command("addosrs", RunMode = RunMode.Async)]
        [Summary("Add an OSRS name.")]
        [RequireContext(ContextType.Guild)]
        public async Task AddOsrsName(string name) {
            var playerDecorater = await _playerService.CoupleDiscordGuildUserToOsrsAccount(GetGuildUser(), name);

            var builder = new EmbedBuilder()
                .AddWiseOldMan(playerDecorater)
                .WithMessageAuthorFooter(Context)
                .WithDescription(
                    $"Coupled {playerDecorater.Item.DisplayName} to your discord account in the server {Context.Guild.Name}");

            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Set Name")]
        [Command("name")]
        [Summary("Set your desired name")]
        public async Task SetName() {

        }

        [Name("Account cycle")]
        [Command("accounts", RunMode = RunMode.Async)]
        [Summary("Cycle through accounts ")]
        [RequireContext(ContextType.Guild)]
        public async Task CycleThroughAccounts() {
            var defaultAccountTask = _playerService.GetDefaultOsrsDisplayName(GetGuildUser());
            var accountDecorators = (await _playerService.GetAllOsrsAccounts(GetGuildUser())).ToList();
            var defaultAccount = await defaultAccountTask;


            if (!accountDecorators.Any()) {
                // we want to update actually.
                _ = SendNoResultMessage(description: "No accounts coupled");
                return;
            }

            var pages = accountDecorators.Select(x => new EmbedBuilder()
                .AddWiseOldMan(x)
                .WithMessageAuthorFooter(Context)
                //.WithDescription($"Oldschool runescape username: {x.Item.DisplayName}.")
                .WithThumbnailUrl(x.Item.Type.WiseOldManIconUrl())
                .AddField("Combat", x.Item.CombatLevel, true)
                //.AddField("Overall", x.Item.LatestSnapshot.GetMetricForType(MetricType.Overall).Level, true)
                .AddField("Account Mode", x.Item.Type, true)
                .AddField("Build", x.Item.Build, true)).ToList();

            var formatString = "Thumbs up to set as main!\r\nCurrent main: {0}";

            var infoMessageTask = ReplyAsync(string.Format(formatString, defaultAccount));
            var message = new CustomPaginatedMessage(new EmbedBuilder().AddCommonProperties().WithMessageAuthorFooter(Context)) {
                Pages = pages,
                Options = new CustomActionsPaginatedAppearanceOptions() {
                    Delete = async (toDelete, i) => {
                        // Delete from original decorator, so our select keeps working #BAD
                        var decorator = accountDecorators[i];
                        await _playerService.DeleteCoupledOsrsAccount(GetGuildUser(), decorator.Item.Id);

                        var newDefault = await _playerService.GetDefaultOsrsDisplayName(GetGuildUser());
                        _ = infoMessageTask.Result.ModifyAsync(props => props.Content = string.Format(formatString, newDefault));
                        accountDecorators.RemoveAt(i);
                    },
                    Select = async (selected, i) => {
                        // Bug with delete?!
                        var playerDecorator = accountDecorators[i];
                        var newDefault = await _playerService.SetDefaultAccount(GetGuildUser(), playerDecorator.Item);

                        // Does this work?!
                        if (infoMessageTask.Result != null) {
                            _ =  infoMessageTask.Result.ModifyAsync(props =>
                                props.Content = string.Format(formatString, newDefault));
                        }
                    }
                }
            };
            await SendPaginatedMessageAsync(message);
        }
    }
}