using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Addons.Interactive.Criteria;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Common.Configuration;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Helpers;
using DiscordBot.Models;
using DiscordBot.Paginator;
using DiscordBot.Services.interfaces;
using DiscordBot.Services.Interfaces;
using DiscordBot.Transformers;
using Serilog.Events;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Output.Exceptions;

namespace DiscordBot.Modules {
    [Name("Player module")]
    public class PlayerModule : BaseWaitMessageEmbeddedResponseModule {
        private const string FormatString =
            "Pencil for a name change!\r\nThumbs up to set as main!\r\nCurrent main: {0}";

        private readonly IPlayerService _playerService;

        public PlayerModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration,
            IPlayerService playerService) : base(mapper, logger, messageConfiguration) {
            _playerService = playerService;
        }

        [Name("Add an OSRS account")]
        [Command("addrsn", RunMode = RunMode.Async)]
        [Summary("Add an OSRS name.")]
        [RequireContext(ContextType.Guild)]
        public async Task AddOsrsName([Remainder] string name) {
            var playerDecorator = await _playerService.CoupleDiscordGuildUserToOsrsAccount(GetGuildUser().ToGuildUserDto(), name);

            var builder = new EmbedBuilder()
                .AddWiseOldMan(playerDecorator)
                .WithMessageAuthorFooter(Context)
                .WithDescription(
                    $"Coupled {playerDecorator.Item.DisplayName} to your discord account in the server {Context.Guild.Name}");

            await ModifyWaitMessageAsync(builder.Build());
        }

        [Name("Account cycle")]
        [Command("accounts", RunMode = RunMode.Async)]
        [Summary("Cycle through accounts ")]
        [RequireContext(ContextType.Guild)]
        public async Task CycleThroughAccounts() {
            var accountDecorators = (await _playerService.GetAllOsrsAccounts(GetGuildUser().ToGuildUserDto())).ToList();
            var defaultAccount = await _playerService.GetDefaultOsrsDisplayName(GetGuildUser().ToGuildUserDto());


            if (!accountDecorators.Any()) {
                // we want to update actually.
                _ = SendNoResultMessage(description: "No accounts coupled");
                return;
            }

            var pages = GetPagesFromAccounts(accountDecorators);
            _ = DeleteWaitMessageAsync();

            //var infoMessage = await ReplyAsync();
            IUserMessage pagedMessage = null;
            // This needs to be refactored! ASAP
            var message =
                new CustomPaginatedMessage(new EmbedBuilder().AddCommonProperties().WithMessageAuthorFooter(Context)) {
                    Pages = pages,
                    Content = string.Format(FormatString, defaultAccount),
                    Options = new CustomActionsPaginatedAppearanceOptions {
                        Delete = async (toDelete, i) => {
                            throw new Exception("tsetr");
                            await DeleteAccount(toDelete, i, accountDecorators, pagedMessage);
                        },
                        Select = async (selected, i) => {
                            await SelectMain(selected, i, accountDecorators, pagedMessage);
                        },
                        EmojiActions = new Dictionary<IEmote, PerformAction> {
                            {
                                new Emoji("✏️"),
                                async (selected, i) => {
                                    await RenameAccount(selected, i, accountDecorators, pagedMessage);
                                }
                            }
                        }
                    }
                };

            pagedMessage = await SendPaginatedMessageAsync(message);
        }

        private List<EmbedBuilder> GetPagesFromAccounts(List<ItemDecorator<Player>> accountDecorators) {
            var pages = accountDecorators.Select(x => new EmbedBuilder()
                .AddWiseOldMan(x)
                .WithMessageAuthorFooter(Context)
                //.WithDescription($"Oldschool runescape username: {x.Item.DisplayName}.")
                .WithThumbnailUrl(x.Item.Type.WiseOldManIconUrl())
                .AddField("Combat", x.Item.CombatLevel, true)
                //.AddField("Overall", x.Item.LatestSnapshot.GetMetricForType(MetricType.Overall).Level, true)
                .AddField("Account Mode", x.Item.Type, true)
                .AddField("Build", x.Item.Build, true)).ToList();
            return pages;
        }

        private async Task DeleteAccount(object selected, int index, List<ItemDecorator<Player>> accounts,
            IUserMessage message) {
            // Delete from original decorator, so our select keeps working #BAD
            var decorator = accounts[index];
            await _playerService.DeleteCoupledOsrsAccount(GetGuildUser().ToGuildUserDto(), decorator.Item.Id);

            var newDefault = await _playerService.GetDefaultOsrsDisplayName(GetGuildUser().ToGuildUserDto());
            _ = message?.ModifyAsync(props => props.Content = string.Format(FormatString, newDefault));
            accounts.RemoveAt(index);
        }

        private async Task SelectMain(object selected, int index, List<ItemDecorator<Player>> accounts,
            IUserMessage message) {
            var playerDecorator = accounts[index];
            var newDefault = await _playerService.SetDefaultAccount(GetGuildUser().ToGuildUserDto(), playerDecorator.Item);

            _ = message?.ModifyAsync(props => props.Content = string.Format(FormatString, newDefault));
        }

        private async Task RenameAccount(object selectedPage, int index, List<ItemDecorator<Player>> accounts,
            IUserMessage message) {
            await Task.Run(async () => {
                var playerDecorator = accounts[index];

                var timeout = TimeSpan.FromMinutes(1);

                var newUserName = await GetNewName(playerDecorator, timeout);
                await RequestNewName(index, accounts, newUserName);
            });
        }

        private Criteria<SocketMessage> BuildCriteria() {
            return new Criteria<SocketMessage>()
                .AddCriterion(new EnsureSourceChannelCriterion())
                .AddCriterion(new EnsureFromUserCriterion(GetGuildUser()));
        }

        private async Task<string> GetNewName(ItemDecorator<Player> playerDecorator, TimeSpan timeout) {
            // Build message
            var criteria = BuildCriteria();
            var infoReply = await Interactive.ReplyAndDeleteAsync(Context,
                $"Please type in the new name for {playerDecorator.Item.DisplayName}", false, null,
                timeout);

            // Send message
            var response = await Interactive.NextMessageAsync(Context, criteria, timeout);
            var newUserName = response.Content;

            // Delete messages
            _ = infoReply.DeleteAsync();
            _ = response.DeleteAsync();
            return newUserName;
        }

        private async Task RequestNewName(int accountIndex, List<ItemDecorator<Player>> accounts, string newUserName) {
            var sendMessage = true;
            string responseString = null;

            try {
                // Handle message
                var nameChange =
                    await _playerService.RequestNameChange(accounts[accountIndex].Item.Username, newUserName);
                responseString =
                    $"Name change requested (ID: {nameChange.Id} - {nameChange.OldName} -> {nameChange.NewName})";
            } catch (BadRequestException e) {
                responseString = $"Something went wrong with the request: {e.Message}";
            } catch (Exception e) {
                responseString = $"{e.GetType().Name} - {e.Message}";
                sendMessage = false;
                throw;
            } finally {
                if (sendMessage) {
                    _ = Context.Channel.SendMessageAsync(responseString);
                }

                _ = Logger.Log(responseString, LogEventLevel.Information);
            }
        }
    }
}
