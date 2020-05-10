using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace DiscordBotFanatic.Services {
    public class LogService {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _discordLogger;
        private readonly ILogger _commandsLogger;
        private readonly ILogger _debugLogger;

        public LogService(DiscordSocketClient discord, CommandService commands, ILoggerFactory loggerFactory)
        {
            _discord = discord;
            _commands = commands;

            _loggerFactory = loggerFactory;
            _discordLogger = _loggerFactory.CreateLogger("discord");
            _commandsLogger = _loggerFactory.CreateLogger("commands");
            _debugLogger = _loggerFactory.CreateLogger("debug");

            _discord.Log += LogDiscord;
            _commands.Log += LogCommand;
            //_debugLogger.Log += LogDebug;
        }
        
        public Task LogDebug(LogMessage message) {
            _debugLogger.Log(
                LogLevelFromSeverity(message.Severity), 
                0, 
                message,
                message.Exception, 
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private Task LogDiscord(LogMessage message)
        {
            _discordLogger.Log(
                LogLevelFromSeverity(message.Severity), 
                0, 
                message,
                message.Exception, 
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private Task LogCommand(LogMessage message)
        {
            //// Return an error message for async commands
            //if (message.Exception is CommandException command)
            //{
            //    // Don't risk blocking the logging task by awaiting a message send; ratelimits!?
            //    var _ = command.Context.Channel.SendMessageAsync($"Error: {command.Message}");
            //}

            _commandsLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
            => (LogLevel)(Math.Abs((int)severity - 5));
    }
}