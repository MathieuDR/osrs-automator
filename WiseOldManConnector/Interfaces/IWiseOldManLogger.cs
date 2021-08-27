using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManLogger {
        Task Log(LogLevel logLevel, Exception e, string message, params object[] arguments);
    }
}
