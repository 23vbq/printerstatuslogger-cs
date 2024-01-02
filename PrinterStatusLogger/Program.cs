using PrinterStatusLogger.CommandHandling;
using PrinterStatusLogger.Config;
using PrinterStatusLogger.PrinterManaging;
using System.Reflection;

namespace PrinterStatusLogger
{
    internal class Program
    {
        public static bool exitCalled = false;
        public static bool userMode = true;

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
            Logger.Log(LogType.INFO, "PrinterStatusLogger v0.1 " + args.ToString());
            PrinterManager printerManager = new PrinterManager();
            ConfigManager configManager = new ConfigManager();
            if (args.Length > 0 )
            {
                if (args.Contains("-u"))
                {
                    userMode = true;
                }
                if (args.Contains("-m"))
                {
                    printerManager.ListPrinterModels();
                    return;
                }
            }
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