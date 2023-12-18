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
            logbuffer = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy") + " [" + type.ToString() + "] " + message;
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
    }
}
