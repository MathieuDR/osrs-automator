using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    [Group("event")]
    [Name("Events")]
    public class EventModule : ModuleBase<SocketCommandContext> {
        private readonly IGuildService _service;
        private IGuildUser _user;

        public EventModule(IGuildService service) {
            _service = service;
        }

        protected override void BeforeExecute(CommandInfo command) {
            if (Context == null) {
                return;
            }

            _user = Context.User as IGuildUser;
            if (_user == null) {
                throw new Exception($"You need to be in a server to use this command.");
            }

            base.BeforeExecute(command);
        }

        [Command("create")]
        [Summary("Create an event")]
        public Task CreateEvent(string name, int minimumPlayersPerCounter = 1, int maximumPlayersPerCounter = 4) {
            // Check permissions
            if (!_service.DoesUserHavePermission(_user, Permissions.EventManager)) {
                throw new UnauthorizedAccessException($"You don't have acces towards event management");
            }

            // Check if one is running
            if (_service.HasActiveEvent(_user.Guild)) {
                throw new Exception($"An guild event is already running on this guild.");
            }

            GuildCustomEvent guildCustomEvent = new GuildCustomEvent(_user, name, minimumPlayersPerCounter, maximumPlayersPerCounter);
            var fromDb = _service.InsertGuildEvent(guildCustomEvent);
            EmbedBuilder embed = new EmbedBuilder() {Title = $"Success!", Description = $"Event {fromDb.Name} created!"};
            return ReplyAsync(embed: embed.Build());
        }

        [Command("running")]
        [Summary("See the running command")]
        public Task OutputRunning() {
            GuildCustomEvent fromDb = GetActiveGuildEvent();

            EmbedBuilder embed = new EmbedBuilder() {Title = $"We found one!", Description = $"Event {fromDb.Name} is running!{Environment.NewLine}It started at {fromDb.CreatedOn}."};

            StringBuilder builder = new StringBuilder($"If you want to add a score counter. You'll need to add a minimum of {fromDb.MinimumPerCounter} player(s) and a maximum of {fromDb.MaximumPerCounter} per score count!");
            builder.Append($"{Environment.NewLine}");
            builder.Append($"Total screenshots shared (Games played): {fromDb.EventCounters.Select(x => x.ImageUrl).Distinct().Count()}");
            
            embed.AddField("Scoring", builder.ToString());
            GuildEventTopCountersToField(fromDb.EventCounters, 5, embed);

            embed.AddField("Command", $"Use the `help Events` command for more information!");

            return ReplyAsync(embed: embed.Build());
        }

        [Command("top")]
        [Summary("See the top of the event")]
        public Task OutputTop(int top) {
            GuildCustomEvent fromDb = GetActiveGuildEvent();

            EmbedBuilder embed = new EmbedBuilder() {Title = $"Top results"};
            GuildEventTopCountersToField(fromDb.EventCounters, top, embed);

            return ReplyAsync(embed: embed.Build());
        }

        [Command("end")]
        [Summary("Ends an event")]
        public Task EndEvent() {
            GuildCustomEvent fromDb = GetActiveGuildEvent();

            if (!_service.EndGuildEvent(fromDb)) {
                throw new Exception($"Something went wrong with ending the event.");
            }
            
            EmbedBuilder embed = new EmbedBuilder() {Title = $"Success!", Description = $"The top 5 players were, Congratulations"};
            GuildEventTopCountersToField(fromDb.EventCounters, 5, embed);

            return ReplyAsync(embed: embed.Build());
        }

        [Command("vet")]
        [Summary("Vets a player and output all imageUrls")]
        [Priority(100)]
        public async Task OutputImages(IUser user) {

            // Check permissions
            if (!_service.DoesUserHavePermission(_user, Permissions.EventManager)) {
                throw new UnauthorizedAccessException($"You don't have acces towards event management");
            }

            GuildCustomEvent fromDb = GetActiveGuildEvent();

            var counts = fromDb.EventCounters.Where(x => x.UserId == user.Id).Select(x => x.ImageUrl).ToList();
            foreach (string url in counts) {
                EmbedBuilder builder = new EmbedBuilder();
                builder.Footer = new EmbedFooterBuilder(){IconUrl = Context.User.GetAvatarUrl(), Text = $"Vetting by {Context.User.Username}"};
                builder.ImageUrl = url;
                await ReplyAsync(embed: builder.Build());
            }
        }

        [Command("vet")]
        [Summary("Removes a count from one or more users")]
        public Task OutputImages(string url, IUser user = null)
        {
            // Check permissions
            if (!_service.DoesUserHavePermission(_user, Permissions.EventManager)) {
                throw new UnauthorizedAccessException($"You don't have acces towards event management");
            }

            GuildCustomEvent fromDb = GetActiveGuildEvent();

            var toDeleteQuery = fromDb.EventCounters.Where(x => x.ImageUrl == url);
            if (user != null) {
                toDeleteQuery = toDeleteQuery.Where(x => x.UserId == user.Id);
            }

            var toDelete = toDeleteQuery.ToList();

            _service.RemoveCounters(fromDb, toDelete);
            EmbedBuilder builder = new EmbedBuilder(){Title = "Success", Description = $"We deleted {toDelete.Count} counters!"};
            return ReplyAsync(embed: builder.Build());
        }

        private void GuildEventTopCountersToField(List<GuildEventCounter> counters, int maximumFields, EmbedBuilder builder) {
            if (counters == null || !counters.Any()) {
                builder.AddField($"No records", "No counting records happened yet!");
                return;
            }

            var top = counters.GroupBy(x => x.UserId).OrderByDescending(x => x.Count()).ToList();
            maximumFields = Math.Min(maximumFields, top.Count);


            for (int i = 0; i < maximumFields; i++) {
                var item = top[i];
                var user = Context.Guild.Users.SingleOrDefault(x => x.Id == item.Key);
                string title = user?.Nickname ?? $"Unknown user ({item.Key})";
                builder.AddField(title, $"Has in total {item.Count()} counts! Congrats");
            }
        }

        private GuildCustomEvent GetActiveGuildEvent() {
            GuildCustomEvent fromDb = _service.GetActiveGuildEvents(_user.Guild).FirstOrDefault();

            if (fromDb == null) {
                throw new Exception($"No event running. Please create one.");
            }

            return fromDb;
        }

        [Command("add")]
        [Summary("Add a counter to the current event")]
        public Task AddCounter([Remainder] UserListWithImageArguments arguments) {
            IGuildUser user = Context.User as IGuildUser;
            if (user == null) {
                throw new Exception($"You need to be in a server to use this command.");
            }

            List<string> usernames = arguments.Users.Select(x => (IGuildUser) x).Select(x => x.Nickname).ToList();
            _service.AddEventCounter(user.Guild, arguments);
            EmbedBuilder embed = new EmbedBuilder() {Title = "Success", Description = $"{string.Join(", ", usernames)}"};
            return ReplyAsync(embed: embed.Build());
        }
    }
}