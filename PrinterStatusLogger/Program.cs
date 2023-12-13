using PrinterStatusLogger.CommandHandling;
using PrinterStatusLogger.Config;
using PrinterStatusLogger.PrinterManaging;

namespace PrinterStatusLogger
{
    internal class Program
    {
        public static bool exitCalled = false;

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
            PrinterManager printerManager = new PrinterManager();
            ConfigManager configManager = new ConfigManager();
            if (args.Length > 0 )
            {
                if (args[0] == "-m")
                {
                    printerManager.ListPrinterModels();
                    return;
                }
            }
            configManager.LoadPrinters(printerManager);
            Console.ReadLine();
        }
    }
}