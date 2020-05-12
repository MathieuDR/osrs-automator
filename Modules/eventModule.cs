using System;
using System.Linq;
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

        public EventModule(IGuildService service) {
            _service = service;
        }

        [Command("create")]
        [Summary("Create an event")]
        public Task CreateEvent(string name, int minimumPlayersPerCounter = 1, int maximumPlayersPerCounter = 4) {
            IGuildUser user = Context.User as IGuildUser;
            if (user == null) {
                throw new Exception($"You need to be in a server to use this command.");
            }

            // Check permissions
            if (!_service.DoesUserHavePermission(user, Permissions.EventManager)) {
                throw new UnauthorizedAccessException($"You don't have acces towards event management");
            }

            // Check if one is running
            if (_service.HasActiveEvent(user.Guild)) {
                throw new Exception($"An guild event is already running on this guild.");
            }
            
            GuildEvent guildEvent = new GuildEvent(user, name, minimumPlayersPerCounter, maximumPlayersPerCounter);
            var fromDb = _service.InsertGuildEvent(guildEvent);
            EmbedBuilder embed = new EmbedBuilder() {Title = $"Success!", Description = $"Event {fromDb.Name} created!"};
            return ReplyAsync(embed: embed.Build());
        }

        [Command("running")]
        [Summary("See the running command")]
        public Task OutputRunning() {
            IGuildUser user = Context.User as IGuildUser;
            if (user == null) {
                throw new Exception($"You need to be in a server to use this command.");
            }

            EmbedBuilder embed;
            GuildEvent fromDb = _service.GetActiveGuildEvents(user.Guild).FirstOrDefault();
            if (fromDb == null) {
                embed = new EmbedBuilder() {Title = $"No running event!", Description = $"Please create one."};
            } else {
                embed = new EmbedBuilder() {Title = $"We found one!", Description = $"Event {fromDb.Name} is running!{Environment.NewLine}It started at {fromDb.CreatedOn}."};
                embed.AddField("Scoring", $"If you want to add a score counter. You'll need to add a minimum of {fromDb.MinimumPerCounter} player(s) and a maximum of {fromDb.MaximumPerCounter} per score count!");
            }

            embed.AddField("Command", $"Use the `help Events` command for more information!");

            return ReplyAsync(embed: embed.Build());
        }

        [Command("add")]
        [Summary("Add a counter to the current event")]
        public Task AddCounter([Remainder]UserListWithImageArguments arguments) {
            IGuildUser user = Context.User as IGuildUser;
            if (user == null) {
                throw new Exception($"You need to be in a server to use this command.");
            }

            _service.AddEventCounter(user.Guild, arguments);
            EmbedBuilder embed = new EmbedBuilder(){Title = "Success", Description = $"{string.Join(", ", arguments.Users.Select(x=> x.Username))}"};
            return ReplyAsync(embed:embed.Build());
        }
    }
}