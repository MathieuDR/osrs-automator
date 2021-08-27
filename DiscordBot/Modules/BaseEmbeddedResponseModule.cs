using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Addons.Interactive;
using Discord.Addons.Interactive.Criteria;
using Discord.Addons.Interactive.Paginator;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Paginator;
using DiscordBot.Services.interfaces;

namespace DiscordBot.Modules {
    [DontAutoLoad]
    public abstract class BaseEmbeddedResponseModule : InteractiveBase<SocketCommandContext> {
        protected BaseEmbeddedResponseModule(Mapper mapper, ILogService logger) {
            Mapper = mapper;
            Logger = logger;
        }

        public Mapper Mapper { get; }
        public ILogService Logger { get; }

        protected override void BeforeExecute(CommandInfo command) {
            if (Context == null) {
                return;
            }

            base.BeforeExecute(command);
        }


        public async Task<IUserMessage> SendPaginatedMessageAsync(PaginatedMessage pager,
            ICriterion<SocketReaction> criterion = null) {
            var callback = new CustomPaginatedMessageCallback(Interactive, Context, pager, criterion);
            await callback.DisplayAsync().ConfigureAwait(false);
            return callback.Message;
        }


        protected PaginatedMessage ConvertToPaginatedMessage(IPageableResponse responseObject) {
            return new() {
                AlternateDescription = responseObject.AlternatedDescription,
                Pages = Mapper.Map<IEnumerable<Embed>>(responseObject.Pages),
                Options = new PaginatedAppearanceOptions {
                    JumpDisplayOptions = JumpDisplayOptions.Never,
                    DisplayInformationIcon = false
                },
                Author = BuildUserAsAuthor(),
                Content = "TEST",
                Color = Color.Red,
                Title = "TITLE"
            };
        }

        protected EmbedAuthorBuilder BuildUserAsAuthor() {
            return new() {
                IconUrl = Context.User.GetAvatarUrl(),
                Name = Context.User.Username
            };
        }

        protected EmbedFooterBuilder BuildUserForFooter() {
            return new() {
                IconUrl = Context.User.GetAvatarUrl(),
                Text = Context.User.Username
            };
        }

        protected IGuildUser GetGuildUser() {
            return Context.User as IGuildUser ?? throw new Exception("User is not in a server.");
        }
    }
}
