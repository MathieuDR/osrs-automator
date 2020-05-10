using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Modules.Parameters;
using DiscordBotFanatic.TypeReaders;

namespace DiscordBotFanatic.Services {
    public class CommandHandlingService {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands) {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider) {
            _provider = provider;
            _commands.AddTypeReader<PeriodAndMetricOsrsArguments>(new PeriodAndMetricOsrsTypeReader());
            _commands.AddTypeReader<PeriodOsrsArguments>(new PeriodOsrsTypeReader());
            _commands.AddTypeReader<MetricOsrsArguments>(new MetricOsrsTypeReader());
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task MessageReceived(SocketMessage rawMessage) {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) {
                return;
            }

            if (message.Source != MessageSource.User) {
                return;
            }

            int argPos = 0;
            if (!message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) {
                return;
            }

            var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

            if (result.Error.HasValue ) {
                string errorResponse = "";
                string helpMessage = "Please use the `help` command for more info.";
                string contactMessage = "Please contact the administrator for help.";
                switch (result.Error) {
                    case CommandError.UnknownCommand:
                        errorResponse = $"Command unknown. {helpMessage}";
                        break;
                    case CommandError.ParseFailed:
                        errorResponse = $"Could not parse argument. {helpMessage}";
                        break;
                    case CommandError.BadArgCount:
                        errorResponse = $"Could not parse argument. {helpMessage}";
                        break;
                    case CommandError.ObjectNotFound:
                        errorResponse = $"Object not found. {contactMessage}";
                        break;
                    case CommandError.MultipleMatches:
                        errorResponse = $"Command has multiple matches. {contactMessage}";
                        break;
                    case CommandError.UnmetPrecondition:
                        errorResponse = $"Insufficient rights. {helpMessage}";
                        break;
                    case CommandError.Exception:
                        errorResponse = $"Exception thrown. {contactMessage}";
                        break;
                    case CommandError.Unsuccessful:
                    case null:
                        errorResponse = $"unsuccessful, please try again later. {helpMessage} If the problem persists {contactMessage.ToLower()}";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                await context.User.SendMessageAsync(errorResponse);
            }
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) {
            // command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified) {
                return;
            }

            // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess) {
                return;
            }

            // the command failed, let's notify the user that something happened.
            await context.Channel.SendMessageAsync($"error: {result}");
        }
    }
}