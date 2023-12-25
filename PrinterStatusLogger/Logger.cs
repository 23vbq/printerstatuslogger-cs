using PrinterStatusLogger.PrinterManaging;

namespace PrinterStatusLogger
{
    public enum LogType
    {
        INFO,
        WARNING,
        ERROR,
        PRNT_INFO
    }
    public static class Logger
    {
        private static StreamWriter _logfileout;
        private static string logbuffer;
        private static Dictionary<LogType, ConsoleColor> logcolor = new Dictionary<LogType, ConsoleColor>()
        {
            {LogType.ERROR, ConsoleColor.DarkRed },
            {LogType.WARNING, ConsoleColor.Yellow }
        };

        static Logger()
        {
            _logfileout = new StreamWriter("syslog", true);
            _logfileout.AutoFlush = true;
            logbuffer = "";
            _logfileout.WriteLine("<======= " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + "=======>");
        }
        /// <summary>
        /// Logs message
        /// </summary>
        /// <param name="type"></param>
        /// <param name="message"></param>
        public static void Log(LogType type, string message)
        {
            logbuffer = BuildLog(type, message);
            SetConsoleColor(type);
            Console.WriteLine(logbuffer);
            _logfileout.WriteLine(logbuffer);
        }
        /// <summary>
        /// Logs printer info
        /// </summary>
        /// <param name="printer"></param>
        /// <param name="tonerLevel"></param>
        public static void Log(Printer printer, int tonerLevel)
        {
            Log(LogType.PRNT_INFO, printer.Address + " Toner: " +  tonerLevel);
        }
        public static string BuildLog(LogType type, string message)
        {
            return DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + " [" + type.ToString() + "] " + message;
        }
        private static void SetConsoleColor(LogType type)
        {
            if (!logcolor.ContainsKey(type))
            {
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }
            Console.ForegroundColor = logcolor[type];
        }
    }
}
