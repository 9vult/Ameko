using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace AssCS
{
    /// <summary>
    /// Logger
    /// </summary>
    public class Logger
    {
        public static Logger Instance = new Logger();
        private readonly ObservableCollection<Log> _logs = new ObservableCollection<Log>();

        /// <summary>
        /// List of logs
        /// </summary>
        public ObservableCollection<Log> Logs => _logs;

        public void Info(string message, string source = "")
        {
            Log log = new Log(message, source, LogLevel.INFO);
            _logs.Add(log);
        }

        public void Debug(string message, string source = "")
        {
            Log log = new Log(message, source, LogLevel.DEBUG);
            _logs.Add(log);
        }

        public void Warn(string message, string source = "")
        {
            Log log = new Log(message, source, LogLevel.WARN);
            _logs.Add(log);
        }

        public void Error(string message, string source = "")
        {
            Log log = new Log(message, source, LogLevel.ERROR);
            _logs.Add(log);
        }

    }

    public class Log
    {
        private readonly LogLevel _logLevel;
        private readonly string _message;
        private readonly string _source;
        private readonly DateTime _timestamp;

        public Log(string message, string source, LogLevel logLevel)
        {
            _source = source;
            _message = message;
            _logLevel = logLevel;
            _timestamp = DateTime.Now;
        }

        public string Message => _message;
        public string Source => _source;
        public LogLevel LogLevel => _logLevel;
        public string LogLevelString => _logLevel.ToString();
        public DateTime Timestamp => _timestamp;
        public string TimeString => _timestamp.ToString("yyyy-MM-ddTHH:mm:ss");

        public override string ToString()
        {
            return $"{_timestamp:yyyy-MM-ddTHH:mm:ss} [{_logLevel}] {(_source != string.Empty ? $"({_source}) " : "")}→ {_message}";
        }

        public string ToString(bool includeSource)
        {
            if (includeSource) return ToString();
            return $"{_timestamp:yyyy-MM-ddTHH:mm:ss} [{_logLevel}] → {_message}";
        }
    }

    public enum LogLevel
    {
        INFO,
        DEBUG,
        WARN,
        ERROR
    }
}
