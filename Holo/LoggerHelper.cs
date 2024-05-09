using AssCS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace Holo
{
    public class LoggerHelper
    {
        private static LoggerHelper instance;
        private readonly string logsRoot;
        private readonly Logger logger;

        private void WriteLogsToFile(IEnumerable<Log> logs)
        {
            if (!logs.Any()) return;
            var filename = Path.Combine(logsRoot, $"log_{logs.First().Timestamp:yyyy-MM-dd}.txt");
            using (StreamWriter writer = System.IO.File.AppendText(filename))
            {
                writer.WriteLine(string.Join(Environment.NewLine, logs));
            }
        }

        public static void Initialize()
        {
            if (instance == null) instance = new LoggerHelper();
        }

        private LoggerHelper()
        {
            logsRoot = Path.Combine(HoloContext.Directories.HoloStateHome, "logs");
            if (!Directory.Exists(logsRoot))
                Directory.CreateDirectory(logsRoot);

            logger = Logger.Instance;
            WriteLogsToFile(logger.Logs);

            logger.Logs.CollectionChanged += (o, e) => WriteLogsToFile(e.NewItems.Cast<Log>());
        }

    }
}
