using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBotFanatic.Services.interfaces;
using Serilog;

namespace DiscordBotFanatic.Services {
    public class SerilogService : ILogService {
        public SerilogService(DiscordSocketClient discord, CommandService commands) {
            discord.Log += LogDiscordClient;
            commands.Log += LogCommand;
        }

        public Task LogDebug(LogMessage message) {
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
            Log.Information($"{area} - {stopwatch.ElapsedMilliseconds}ms");
            return Task.CompletedTask;
        }

        private void LogFromLogMessage(LogMessage message) {
            switch (message.Severity) {
                case LogSeverity.Critical:
                    Log.Fatal(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Error:
                    Log.Error(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Warning:
                    Log.Warning(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Info:
                    Log.Information(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Verbose:
                    Log.Verbose(message.Exception, message.Message, message.Source);
                    break;
                case LogSeverity.Debug:
                    Log.Debug(message.Exception, message.Message, message.Source);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}