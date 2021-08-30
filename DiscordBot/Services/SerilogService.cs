using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services.Interfaces;
using Serilog.Events;

namespace DiscordBot.Services {
    public class SerilogService : ILogService {
        public SerilogService(DiscordSocketClient discord, CommandService commands) {
            discord.Log += LogDiscordClient;
            commands.Log += LogCommand;
        }

        public Task Log(LogMessage message) {
            LogFromLogMessage(message);
            return Task.CompletedTask;
        }

        public Task LogDiscordClient(LogMessage message) {
            LogFromLogMessage(message);
            return Task.CompletedTask;
        }

        public Task LogCommand(LogMessage message) {
            LogFromLogMessage(message);
            return Task.CompletedTask;
        }

        public Task LogStopWatch(string area, Stopwatch stopwatch) {
            Serilog.Log.Information($"{area} - {stopwatch.ElapsedMilliseconds}ms");
            return Task.CompletedTask;
        }

        public Task Log(string message, LogEventLevel level, params object[] arguments) {
            return Log(message, level, null, arguments);
        }

        public Task Log(string message, LogEventLevel level, Exception exception, params object[] arguments) {
            Serilog.Log.Write(level, exception, message, arguments);
            return Task.CompletedTask;
        }

        public Task LogWithCommandInfoLine(string message, LogEventLevel level, Exception exception, params object[] arguments) {
            var formattedMessage = "[{CommandContextDto}] " + message;
            return Log(formattedMessage, level, exception, arguments);
        }

        private void LogFromLogMessage(LogMessage message) {
            switch (message.Severity) {
                case LogSeverity.Critical:
                    Serilog.Log.Fatal(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Error:
                    Serilog.Log.Error(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Warning:
                    Serilog.Log.Warning(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Info:
                    Serilog.Log.Information(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Verbose:
                    Serilog.Log.Verbose(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Debug:
                    Serilog.Log.Debug(message.Exception, message.Message, message.Source);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
