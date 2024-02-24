//using PrinterStatusLogger.CommandHandling;
using PrinterStatusLogger.Config;
using PrinterStatusLogger.PrinterManaging;

namespace PrinterStatusLogger
{
    internal class Program
    {
        public const string version = "0.4";

        static PrinterManager printerManager = null; // Is this safe?
        static ConfigManager configManager = null;   // Maybe need to make this classes as static

        public static bool userMode { private set; get; } = false;
        public static bool noAlertMode { private set; get; } = false;
        public static bool verboseMode { private set; get; } = false;

        private static readonly Dictionary<string, Action> s_programArgs = new Dictionary<string, Action>
        {
            { "-h", () => { PrintHelp(); } },
            { "-u", () => { userMode = true; } },
            { "-na", () => { noAlertMode = true; } },
            { "-m",  ModelListingMode},
            { "-v", () => { verboseMode = true; } }
        };

        /*public static readonly Dictionary<string, UInt16> s_wellKnownPorts = new Dictionary<string, ushort> // Probably not needed
        {
            { "http://", 80 },
            { "https://", 443 }
        };*/

        static void Main(string[] args)
        {
            Logger.Log(LogType.INFO, "PrinterStatusLogger v" + version + " " + string.Join(' ', args));
            printerManager = new PrinterManager();
            configManager = new ConfigManager();
            /*
             * Args check
             */
            if (args.Length > 0 )
            {
                int n = args.Length;
                for (int i = 0; i < n; i++)
                {
                    if (s_programArgs.ContainsKey(args[i]))
                        s_programArgs[args[i]].Invoke();
                    else
                        PrintHelp(args[i]);
                }
            }
            /*
             * Activated flags info
             */
            if (userMode)
                Logger.Log(LogType.WARNING, "UserMode is activated. Program will be able to wait for user input!");
            if (noAlertMode)
                Logger.Log(LogType.WARNING, "NoAlertMode is activated. No alerts will be sent!");
            if (verboseMode)
                Logger.Log(LogType.WARNING, "VerboseMode is activated. Program will return more logs.");
            /*
             * Program part
             */
            try
            {
                configManager.LoadPrinterModels(printerManager.RegisterPrinterModel);
                configManager.LoadPrinters(printerManager);
                Alerter.Initialize(configManager.GetAlerterCreds(), configManager.LoadAlerter);
                Logger.Log(LogType.INFO, "Starting printers scan");
                printerManager.RunPrinterScan();
                Alerter.Send();
            } catch (Exception ex)
            {
                FatalError(ex);
            }
        }
        static void FatalError(Exception ex)
        {
            Logger.Log(LogType.ERROR, ex.Message);
            Logger.Log(LogType.ERROR, "Fatal error! Aborting...");
            Environment.Exit(1);
        }
        /// <summary>
        /// Prints help only to console output
        /// </summary>
        /// <param name="unknownArg">If unknownArg is specified it will print this with invalid argument info</param>
        static void PrintHelp(string? unknownArg = null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (unknownArg != null )
                Console.Write("Invalid argument: " + unknownArg + "\n");
            Console.WriteLine("Usage: printerstatuslogger.exe [arguments]");
            Console.WriteLine("\t-h\tHelp - shows this dialog and exits");
            Console.WriteLine("\t-u\tUser Mode - Program can ask user certain things (like creating default config), and wait for user input");
            Console.WriteLine("\t-na\tNo Alert Mode - Disables whole Alerter module");
            Console.WriteLine("\t-m\tModel List - Only lists loaded printer models and then exits");
            Console.WriteLine("\t-v\tVerbose Mode - Program will return more logs");
            Console.WriteLine("\nDocs: https://github.com/23vbq/printerstatuslogger-cs");
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        }
        static void ModelListingMode()
        {
            try
            {
                configManager.LoadPrinterModels(printerManager.RegisterPrinterModel);
            }
            catch (Exception ex)
            {
                FatalError(ex);
            }
            printerManager.ListPrinterModels();
            Logger.Log(LogType.WARNING, "Model listing mode. Aborting other work...");
            Environment.Exit(0);
        }
    }
}