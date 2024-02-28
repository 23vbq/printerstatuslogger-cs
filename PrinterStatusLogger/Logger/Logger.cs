using PrinterStatusLogger.PrinterManaging;
using System.Collections;

namespace PrinterStatusLogger.Logging
{
    public static class Logger
    {
        private static StreamWriter _logfileout;
        private static string logbuffer;
        private static Dictionary<LogType, ConsoleColor> logcolor = new Dictionary<LogType, ConsoleColor>()
        {
            {LogType.V_INFO, ConsoleColor.Gray },
            {LogType.ERROR, ConsoleColor.DarkRed },
            {LogType.WARNING, ConsoleColor.Yellow },
            {LogType.V_WARNING, ConsoleColor.DarkYellow },
            {LogType.PRNT_INFO, ConsoleColor.Green }
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
            if (Program.verboseMode > type)
                return;
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
        /// <summary>
        /// Builds log in default format
        /// </summary>
        /// <param name="type">Type of log</param>
        /// <param name="message">Message to send in log</param>
        /// <returns></returns>
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
        /// <summary>
        /// Checks if log is verbose type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool IsVerbose(LogType type)
        {
            return type == LogType.V_INFO ||
                   type == LogType.V_WARNING;
        }
        /// <summary>
        /// Creates string containing hex representation of bools array. It allows to easy check lot of bools with error code output.<br></br>
        /// Debug Info: Bits are reversed, so 0x01 means {false, true, true} | or maybe its not :P
        /// </summary>
        /// <param name="bools">Array to check</param>
        /// <param name="byte_size">Amount of bytes to store bits</param>
        /// <returns>Hex representation of reversed array in byte size string.<br></br>Ex. byte_size = 1 - "0x04"</returns>
        public static string BitCheck(bool[] bools, Int32 byte_size)
        {
            BitArray ba = new BitArray(bools);
            byte[] code = new byte[byte_size];
            ba.CopyTo(code, 0);
            return "0x" + BitConverter.ToString(code);
        }
    }
}
