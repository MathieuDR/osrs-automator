using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Serilog.Events;

namespace DiscordBot.Services.interfaces {
    public interface ILogService {
        Task Log(LogMessage message);
        Task LogDiscordClient(LogMessage message);
        Task LogCommand(LogMessage message);
        Task LogStopWatch(string area, Stopwatch stopwatch);


        Task Log(string message, LogEventLevel level, params object[] arguments);
        Task Log(string message, LogEventLevel level, Exception exception, params object[] arguments);
        Task LogWithCommandInfoLine(string message, LogEventLevel level, Exception exception, params object[] arguments);
    }
}