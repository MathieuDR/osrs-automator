using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Modules;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using DiscordBotFanatic.Services.interfaces;
using DiscordBotFanatic.TypeReaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord.Net;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models;
using Serilog.Context;
using Serilog.Events;

namespace DiscordBotFanatic.Services {
    public class CommandHandlingService {
        private readonly CommandService _commands;
        private readonly BotConfiguration _configuration;
        private readonly DiscordSocketClient _discord;
        private readonly ILogService _logger;
        private readonly MetricSynonymsConfiguration _metricSynonymsConfiguration;
        private IServiceProvider _provider;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands,
            BotConfiguration configuration, MetricSynonymsConfiguration metricSynonymsConfiguration, ILogService logger) {
            _discord = discord;
            _commands = commands;
            _provider = provider;
            _configuration = configuration;
            _metricSynonymsConfiguration = metricSynonymsConfiguration;
            _logger = logger;

            _discord.MessageReceived += HandleCommandAsync;
            _discord.UserJoined += HandleJoinAsync;
            _commands.CommandExecuted += OnCommandExecutedAsync;
        }

        public async Task InitializeAsync(IServiceProvider provider) {
            _provider = provider;
            _commands.AddTypeReader<PeriodAndMetricArguments>(new PeriodAndMetricOsrsTypeReader(_metricSynonymsConfiguration));
            _commands.AddTypeReader<PeriodArguments>(new PeriodOsrsTypeReader());
            _commands.AddTypeReader<UserListWithImageArguments>(new UserListWithImageArgumentsTypeReader());
            _commands.AddTypeReader<MetricArguments>(new MetricOsrsTypeReader(_metricSynonymsConfiguration));
            _commands.AddTypeReader<BaseArguments>(new BaseArgumentsTypeReader());

            var t = _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
            await _commands.AddModuleAsync<PlayerModule>(provider);
            await _commands.AddModuleAsync<AdminModule>(provider);
            await _commands.AddModuleAsync<CompetitionModule>(provider);
            await _commands.AddModuleAsync<GroupModule>(provider);
            //await _commands.AddModuleAsync<PlayerStatsModule>(provider);
            //await _commands.AddModuleAsync<GroupStatsModule>(provider);
            await t;
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result) {
            // We have access to the info   rmation of the command executed,
            // the context of the command, and the result returned from the
            // execution in this event.

            // We can tell the user what went wrong
            if (!string.IsNullOrEmpty(result?.ErrorReason)) {
                await CreateErrorMessage(context, result);
            }

            // ...or even log the result (the method used should fit into
            // your existing log handler)
            await _logger.LogWithCommandInfoLine($"Command executed.", LogEventLevel.Information, null);
        }

        public async Task CreateErrorMessage(ICommandContext context, IResult result) {
            await _logger.Log(new LogMessage(LogSeverity.Error, "",
                $"{result.Error} - {result.ErrorReason} ({context.Message.Content})"));

            var builder = CreateErrorEmbedBuilder(context, result);


            await context.Channel.SendMessageAsync(embed: builder.Build());
        }

        private Task HandleJoinAsync(SocketGuildUser arg) {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }


        private async Task HandleCommandAsync(SocketMessage messageParam) {
            // Don't process the command if it was a system message
            if (!(messageParam is SocketUserMessage message)) {
                return;
            }

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (message.Content.Trim() == _configuration.CustomPrefix.Trim() ||
                message.Content.Trim() == _discord.CurrentUser.Mention) {
                var commands = _commands.Search("help").Commands.ToList();

                await commands.FirstOrDefault().ExecuteAsync(new SocketCommandContext(_discord, message),
                    new List<object>() {null}, new List<object>() {null}, _provider);

                return;
            }

            if (!(message.HasStringPrefix(_configuration.CustomPrefix + " ", ref argPos) ||
                  message.HasMentionPrefix(_discord.CurrentUser, ref argPos)) || message.Author.IsBot) {
                return;
            }


            // Create a WebSocket-based command context based on the message
            SocketCommandContext context = new SocketCommandContext(_discord, message);

            // Setting logging information.
            using (LogContext.PushProperty("CommandContextDto", new SerilogCommandContextDto(context))) {
                var logTask = _logger.LogWithCommandInfoLine($"Command received.", LogEventLevel.Information, null);

                // Execute the command with the command context we just
                // created, along with the service provider for precondition checks.

                // Keep in mind that result does not indicate a return value
                // rather an object stating if the command executed successfully.
                await _commands.ExecuteAsync(context: context, argPos: argPos, services: _provider);
                await logTask;
            }
        }

        private EmbedBuilder CreateErrorEmbedBuilder(ICommandContext context, IResult result) {
            EmbedBuilder builder = new EmbedBuilder().AddCommonProperties().AddFooterFromMessageAuthor(context);

            builder.Title = "Uh oh! Something went wrong.";

            Debug.Assert(result.Error != null, "result.Error != null");
            builder.AddField(result.Error.Value.ToString(), result.ErrorReason);

            if (result.Error == CommandError.BadArgCount || result.Error == CommandError.ParseFailed) {
                HelpModule.AddStandardParameterInfo(builder, _configuration.CustomPrefix);
            }

            builder.AddField($"Get more help", $"Please use `{_configuration.CustomPrefix} help` for this bot's usage");
            return builder;
        }
    }
}