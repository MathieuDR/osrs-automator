using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Services.interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBotFanatic.Services {
    public class LogService :ILogService {
        private readonly ILogger _discordLogger;
        private readonly ILogger _commandsLogger;
        private readonly ILogger _debugLogger;

        public LogService(DiscordSocketClient discord, CommandService commands, ILoggerFactory loggerFactory)
        {
            _discordLogger = loggerFactory.CreateLogger("discord");
            _commandsLogger = loggerFactory.CreateLogger("commands");
            _debugLogger = loggerFactory.CreateLogger("debug");

            discord.Log += LogDiscordClient;
            commands.Log += LogCommand;
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

        public Task LogDiscordClient(LogMessage message)
        {
            _discordLogger.Log(
                LogLevelFromSeverity(message.Severity), 
                0, 
                message,
                message.Exception, 
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        public Task LogCommand(LogMessage message)
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