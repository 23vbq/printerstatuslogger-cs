using PrinterStatusLogger.CommandHandling;
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
            printerManager.ListPrinterModels();
        }
    }
}