using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigiDiscord.Utilities
{
    public enum LogLevel
    {
        Debug,
        Verbose,
        Info,
        Warning,
        Error
    }

    public abstract class ILogger
    {
        public abstract void Log(LogLevel level, string logLine);

        public void Error(string logLine) { Log(LogLevel.Error, logLine); }
        public void Warning(string logLine) { Log(LogLevel.Warning, logLine); }
        public void Info(string logLine) { Log(LogLevel.Info, logLine); }
        public void Verbose(string logLine) { Log(LogLevel.Verbose, logLine); }
        public void Debug(string logLine) { Log(LogLevel.Debug, logLine); }
    }
}
