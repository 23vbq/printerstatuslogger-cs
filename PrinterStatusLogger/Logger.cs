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

        public static Logger()
        {

        }
        public static void Log(LogType type, string message)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm::ss dd/MM/yyyy") + "[" + type.ToString() + "]\t" + message);
        }
        public static void Log(Printer printer, int tonerLevel)
        {
            Log(LogType.PRNT_INFO, printer.Address + " Toner: " +  tonerLevel);
        }
    }
}
