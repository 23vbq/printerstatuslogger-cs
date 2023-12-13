using PrinterStatusLogger.PrinterManaging;

namespace PrinterStatusLogger.Config
{
    public class ConfigManager
    {
        private readonly string _printersConfigFilename = "printers.cfg";

        public void LoadPrinters(PrinterManager manager)
        {
            if (!configExists(_printersConfigFilename)){
                Console.WriteLine("Printers config not found!");
                CreateConfigFile(_printersConfigFilename);
                Console.WriteLine("Created config file");
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
            string line;
            int loaded = 0;
            using (StreamReader sr = new StreamReader(Path.Combine("Config", filename)))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.StartsWith('#'))
                        continue;
                    if (line.Length < 1)
                        continue;
                    string[] args = line.Split(' '); // TODO log below invalid cases
                    if (function.Invoke(args))
                        loaded++;
                }
            }
            Console.WriteLine("Loaded objects form config: " + loaded);
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

        private bool configExists(string filename)
        {
            return File.Exists(Path.Combine("Config", filename));
        }
    }
}
