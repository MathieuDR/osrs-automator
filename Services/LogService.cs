using System;
using System.Diagnostics;
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
           _commandsLogger.Log(
                LogLevelFromSeverity(message.Severity),
                0,
                message,
                message.Exception,
                (_1, _2) => message.ToString(prependTimestamp: false));
            return Task.CompletedTask;
        }

        public Task LogStopWatch(string area, Stopwatch stopwatch) {
            LogMessage lgMessage = new LogMessage(LogSeverity.Info, area, $"Timer: {stopwatch.ElapsedMilliseconds}ms");
            return LogDebug(lgMessage);
        }

        private static LogLevel LogLevelFromSeverity(LogSeverity severity)
            => (LogLevel)(Math.Abs((int)severity - 5));
    }
}