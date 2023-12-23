using PrinterStatusLogger.PrinterManaging;
using System.Security.Cryptography;

namespace PrinterStatusLogger.Config
{
    public class ConfigManager
    {
        private readonly string _printersConfigFilename = "printers.cfg";

        public void LoadPrinters(PrinterManager manager)
        {
            if (!configExists(_printersConfigFilename)){
                Logger.Log(LogType.WARNING, "Printers config not found");
                if (Program.userMode && AskCreatingDefaultConfig())
                {
                    CreateConfigFile(_printersConfigFilename);
                    Logger.Log(LogType.INFO, "File " + _printersConfigFilename + " created at " + Path.GetFullPath(Path.Combine("Config", _printersConfigFilename)));
                }
                else
                    throw new Exception("Config file not found");
            }
            ReadConfig(_printersConfigFilename, (args) =>
            {
                if (args.Length != 3)
                    return false;
                manager.AddPrinter(args[0], args[1], Int32.Parse(args[2])); // TODO handle invalid parse
                return true;
            });
        }
        /// <summary>
        /// Reads lines from file and splits by space.
        /// Ignores lines that starts with #
        /// </summary>
        /// <param name="filename">Name of file to read</param>
        /// <param name="function">Function to perform on loaded line data</param>
        private void ReadConfig(string filename, Func<string[], bool> function)
        {
            Logger.Log(LogType.INFO, "Reading config file: " + filename);
            string line;
            int loaded = 0;
            using (StreamReader sr = new StreamReader(Path.Combine("Config", filename)))
            {
                int n = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    n++;
                    if (line.StartsWith('#'))
                        continue;
                    if (line.Length < 1)
                        continue;
                    string[] args = line.Split('\t'); // TODO log below invalid cases
                    if (function.Invoke(args))
                        loaded++;
                    else
                        Logger.Log(LogType.ERROR, "Invalid arguments in " + filename + " at line " + n);
                }
            }
            Logger.Log(LogType.INFO, "Loaded objects form config: " + loaded);
        }
        private void CreateConfigFile(string filename)
        {
            if (!Directory.Exists("Config"))
                Directory.CreateDirectory("Config");
            using (StreamWriter sw = File.CreateText(Path.Combine("Config", filename)))
            {
                sw.WriteLine("Default");
            }
        }
        private bool AskCreatingDefaultConfig()
        {
            Console.Write("Do you want to create default config file? (y/n) : ");
            ConsoleKeyInfo key;
            while (true)
            {
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N)
                    break;
                Console.Write("\nInvalid option. (y/n) : ");
            }
            return key.Key == ConsoleKey.Y;
        }

        private bool configExists(string filename)
        {
            return File.Exists(Path.Combine("Config", filename));
        }
    }
}
