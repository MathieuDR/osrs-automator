using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Discord.Addons.Interactive;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
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

        [Name("Set Default OSRS username")]
        [Command("setosrs", RunMode = RunMode.Async)]
        [Summary("Set your default OSRS name for commands, and competitions")]
        public async Task SetOsrsName(string name) {
            //var player = await _playerService.CoupleDiscordGuildUserToOsrsAccount(GetGuildUser(), name);
            //var b = Context.CreateCommonWiseOldManEmbedBuilder();
            //b.Description = $"Successfully set the default player to {player.DisplayName}";
            //await ModifyWaitMessageAsync(b.Build());
        }

        [Name("Add an OSRS account")]
        [Command("addosrs", RunMode = RunMode.Async)]
        [Summary("Add an OSRS name.")]
        [RequireContext(ContextType.Guild)]
        public async Task AddOsrsName(string name) {
            var player = await _playerService.CoupleDiscordGuildUserToOsrsAccount(GetGuildUser(), name);
            var builder = Context.CreateCommonWiseOldManEmbedBuilder();
            builder.WithDescription($"Coupled {player.DisplayName} to your discord account in the server {Context.Guild.Name}");
            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Account cycle")]
        [Command("accounts", RunMode = RunMode.Async)]
        [Summary("Cycle through accounts ")]
        [RequireContext(ContextType.Guild)]
        public async Task CycleThroughNames() {
            var pages = (await _playerService.GetAllOsrsAccounts(GetGuildUser())).Select(x => x.ToPlayerInfoString());

            if (!pages.Any()) {
                var builder = Context.CreateCommonEmbedBuilder();
                builder.WithDescription($"No accounts coupled");
                builder.Title = "Uh oh :(";
                ModifyWaitMessageAsync(builder.Build());
                return;
            }

            var message = new CustomPaginatedMessage(Context.CreateCommonWiseOldManEmbedBuilder()) {
                Pages = pages,
                Options = new DeletePaginatedAppearanceOptions() {
                    JumpDisplayOptions = JumpDisplayOptions.Never,
                    DisplayInformationIcon = false,
                    Delete = async (toDelete, i) => await _playerService.DeleteCoupleOsrsAccountAtIndex(GetGuildUser(), i)
                }
            };
            await SendPaginatedMessageAsync(message);
        }
    }
}