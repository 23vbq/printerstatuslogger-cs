//using PrinterStatusLogger.CommandHandling;
using PrinterStatusLogger.Config;
using PrinterStatusLogger.PrinterManaging;

namespace PrinterStatusLogger
{
    internal class Program
    {
        public const string version = "0.3";

        static PrinterManager printerManager = null; // Is this safe?
        static ConfigManager configManager = null;   // Maybe need to make this classes as static

        //public static bool exitCalled = false; // OLD FOR COMMAND HANDLER
        public static bool userMode = false;
        public static bool noAlertMode = false;

        private static readonly Dictionary<string, Action> programArgs = new Dictionary<string, Action>
        {
            { "-h", () => { PrintHelp(); } },
            { "-u", () => { userMode = true; } },
            { "-na", () => { noAlertMode = true; } },
            { "-m",  ModelListingMode}
        };

        static void Main(string[] args)
        {
            /*CommandHandler commandHandler = new CommandHandler();
            if (args.Length == 0 )
            {
                while ( !exitCalled )
                {
                    Console.Write("> ");
                    string command = Console.ReadLine();
                    commandHandler.Handle(command.Split(' '));
                }
                return;
            }
            commandHandler.Handle(args);*/
            Logger.Log(LogType.INFO, "PrinterStatusLogger v" + version + " " + args.ToString());
            printerManager = new PrinterManager();
            configManager = new ConfigManager();
            /*
             * Args check
             */
            if (args.Length > 0 )
            {
                foreach ( string arg in args )
                {
                    if (programArgs.ContainsKey(arg))
                        programArgs[arg].Invoke();
                    else
                        PrintHelp(arg);
                }
            }
            /*
             * Activated flags info
             */
            if (userMode)
                Logger.Log(LogType.WARNING, "UserMode is activated. Program will be able to wait for user input!");
            if (noAlertMode)
                Logger.Log(LogType.WARNING, "NoAlertMode is activated. No alerts will be sent!");
            /*
             * Program part
             */
            try
            {
                configManager.NEW_LoadPrinterModels(printerManager.RegisterPrinterModel);
                //configManager.LoadPrinterModels(printerManager.RegisterPrinterModel);
                configManager.LoadPrinters(printerManager);
                Alerter.Initialize(configManager.GetAlerterCreds(), configManager.NEW_LoadAlerter);
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