﻿using PrinterStatusLogger.CommandHandling;
using PrinterStatusLogger.Config;
using PrinterStatusLogger.PrinterManaging;

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
            configManager.LoadPrinters(printerManager);
            Logger.Log(LogType.INFO, "Starting printers scan");
            //Console.WriteLine("Name\tToner Level");
            printerManager.RunPrinterScan();
            Console.ReadLine();
        }
    }
}