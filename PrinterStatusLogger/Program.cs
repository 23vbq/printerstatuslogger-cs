//using PrinterStatusLogger.CommandHandling;
using PrinterStatusLogger.Config;
using PrinterStatusLogger.PrinterManaging;

namespace PrinterStatusLogger
{
    internal class Program
    {
        public const string version = "0.2";

        public static bool exitCalled = false;
        public static bool userMode = true;
        public static bool noAlertMode = true;

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
            PrinterManager printerManager = new PrinterManager();
            ConfigManager configManager = new ConfigManager();
            /*
             * Args check
             */
            if (args.Length > 0 )
            {
                if (args.Contains("-u"))
                    userMode = true;
                if (args.Contains("-m"))
                {
                    printerManager.ListPrinterModels();
                    Logger.Log(LogType.WARNING, "Listing model mode. Aborting other work...");
                    Environment.Exit(0);
                }
                if (args.Contains("-na"))
                    noAlertMode = true;
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
                configManager.LoadPrinterModels(printerManager.RegisterPrinterModel);
                configManager.LoadPrinters(printerManager);
                Alerter.Initialize(configManager.GetAlerterCreds(), configManager.LoadAlerter);
                Logger.Log(LogType.INFO, "Starting printers scan");
                printerManager.RunPrinterScan();
                Alerter.Send();
            } catch (Exception ex)
            {
                Logger.Log(LogType.ERROR, ex.Message);
                Logger.Log(LogType.ERROR, "Fatal error! Aborting...");
                Environment.Exit(1);
            }
        }
    }
}