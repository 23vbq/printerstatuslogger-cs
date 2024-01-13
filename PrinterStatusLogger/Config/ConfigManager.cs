﻿using PrinterStatusLogger.PrinterManaging;
using System.Reflection;
using Windows.Security.Credentials;

namespace PrinterStatusLogger.Config
{
    public class ConfigManager
    {
        private readonly string _printersConfigFilename = "printers.cfg";
        private readonly string _alerterConfigFilename = "alerter.cfg";

        private readonly string _DEF_printersConfigResourceName = "PrinterStatusLogger.Config.DefaultConfig.printers.cfg.def";
        private readonly string _DEF_alerterConfigFilename = "PrinterStatusLogger.Config.DefaultConfig.alerter.cfg.def";

        public void LoadPrinters(PrinterManager manager)
        {
            string path = Path.Combine("Config", _printersConfigFilename);
            if (!configExists(_printersConfigFilename)){
                Logger.Log(LogType.WARNING, "Printers config not found");
                if (Ask("Do you want to create default config file?"))
                {
                    CreateConfigFile(_printersConfigFilename, _DEF_printersConfigResourceName);
                    Logger.Log(LogType.INFO, "File " + _printersConfigFilename + " created at " + Path.GetFullPath(Path.Combine("Config", _printersConfigFilename)));
                }
                else
                    throw new Exception("Config file not found");
            }
            ReadConfig(path, (line) =>
            {
                string[] args = line.Split('\t');
                if (args.Length != 3)
                    return false;
                manager.AddPrinter(args[0], args[1], args[2]); // TODO handle invalid parse
                return true;
            });
        }
        public void LoadAlerter()
        {
            string path = Path.Combine("Config", _alerterConfigFilename);
            if (!configExists(_alerterConfigFilename))
            {
                Logger.Log(LogType.WARNING, "Alerter config not found");
                if (Program.userMode && Ask("Do you want to create default config file?"))
                {
                    CreateConfigFile(_alerterConfigFilename, _DEF_alerterConfigFilename);
                    Logger.Log(LogType.INFO, "File " + _alerterConfigFilename + " created at " + Path.GetFullPath(Path.Combine("Config", _alerterConfigFilename)));
                }
                else
                    throw new Exception("Config file not found");
            }
            ReadConfig(path, (line) =>
            {
                string[] args = line.Split('=', 2);
                if (args[0] == "server")
                {
                    Alerter.SmtpServer = args[1];
                    return true;
                }
                if (args[0] == "port")
                {
                    try
                    {
                        Alerter.SmtpPort = Int32.Parse(args[1]);
                        return true;
                    } catch (Exception ex)
                    {
                        Logger.Log(LogType.ERROR, ex.Message);
                        return false;
                    }
                }
                if (args[0] == "recipients")
                {
                    Alerter.MessageRecipients = args[1];
                    return true;
                }
                if (args[0] == "minTonerLevel")
                {
                    int x = -1;
                    int.TryParse(args[1], out x);
                    if (x == -1)
                        return false;
                    Alerter.minTonerLevel = x;
                    return true;
                }
                return false;
            });
        }
        /*
         * Loading models
         */
        public void LoadPrinterModels(Action<PrinterModel> registerModel) // TODO rewrite
        {
            string[] files;
            try
            {
                files = GetPrinterModelConfigFiles();
            } catch (Exception ex)
            {
                //Logger.Log(LogType.ERROR, ex.Message);
                //throw new Exception("Fatal error");
                throw ex;
            }
            int n = 0;
            foreach (string file in files)
            {
                string id = "";
                string name = "";
                string readtonerlevelpath = "";
                string readtonerlevelregex = "";
                ReadConfig(file, (line) =>
                {
                    string[] args = line.Split('=', 2);
                    if (args[0] == "id")
                    {
                        id = args[1];
                        return true;
                    }
                    if (args[0] == "name")
                    {
                        name = args[1];
                        return true;
                    }
                    if (args[0] == "readtonerlevelpath")
                    {
                        readtonerlevelpath = args[1];
                        return true;
                    }
                    if (args[0] == "readtonerlevelregex")
                    {
                        readtonerlevelregex = args[1];
                        return true;
                    }
                    return false;
                });
                string hex = Logger.BitCheck(new bool[]
                {
                    id == "",
                    name == "",
                    readtonerlevelpath == "",
                    readtonerlevelregex == ""
                }, 1);
                if (hex != "0x00")
                {
                    Logger.Log(LogType.ERROR, "Not all arguments specified for model " + file + ": Check code - " + hex);
                    continue;
                }
                PrinterModel buffer = new PrinterModel(id, name, readtonerlevelpath, readtonerlevelregex);
                registerModel.Invoke(buffer);
                n++;
            }
            Logger.Log(LogType.INFO, "Loaded printer models: " + n);
        }
        private string[] GetPrinterModelConfigFiles()
        {
            if (!Directory.Exists("Models"))
                throw new Exception("Models config directory doesn't exists.");
            string[] files = Directory.GetFiles("Models");
            if (files.Length == 0)
                throw new Exception("Models config directory is empty");
            return files;
        }
        /*
         * Windows Credentials
         */
        public PasswordCredential GetAlerterCreds()
        {
            if (Program.noAlertMode)
                return null;
            PasswordVault vault = new PasswordVault();
            IReadOnlyList<PasswordCredential>? credList = null;
            try
            {
                credList = vault.FindAllByResource("PrinterStatusLogger_Alerter");
            } catch
            {
                Logger.Log(LogType.WARNING, "Alerter Credentials not found");
            }
            if (credList == null || credList.Count == 0)
            {
                if (Program.userMode && Ask("Do you want to set credentials for Alerter?"))
                {
                    SetAlerterCreds(vault);
                    return GetAlerterCreds();
                }
                else
                    throw new Exception("Credentials for alerter doesn't exists.");
            }
            return credList[0];
        }
        private void SetAlerterCreds(PasswordVault vault)
        {
            Console.WriteLine("Username: ");
            string un = Console.ReadLine();
            Console.WriteLine("Password: ");
            string pass = Console.ReadLine();
            vault.Add(new PasswordCredential("PrinterStatusLogger_Alerter", un, pass));
        }
        /// <summary>
        /// Reads lines from file and splits by space.
        /// Ignores lines that starts with #
        /// </summary>
        /// <param name="filename">Name of file to read</param>
        /// <param name="function">Function to perform on loaded line data</param>
        private void ReadConfig(string path, Func<string, bool> function)
        {
            Logger.Log(LogType.INFO, "Reading config file: " + path);
            string line;
            int loaded = 0;
            using (StreamReader sr = new StreamReader(path))
            {
                int n = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    n++;
                    if (line.StartsWith('#'))
                        continue;
                    if (line.Length < 1)
                        continue;
                     // TODO log below invalid cases
                    if (function.Invoke(line))
                        loaded++;
                    else
                        Logger.Log(LogType.ERROR, "Invalid setting in " + path + " at line " + n);
                }
            }
            Logger.Log(LogType.INFO, "Loaded objects form config: " + loaded);
        }
        /// <summary>
        /// Creates default config file by copying embedded resource to a file
        /// </summary>
        /// <param name="filename">Name of file to write</param>
        /// <param name="resourceName">Resource name to copy</param>
        /// <exception cref="Exception">Throws exception if resource stream is null</exception>
        private void CreateConfigFile(string filename, string resourceName)
        {
            if (!Directory.Exists("Config"))
                Directory.CreateDirectory("Config");
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream resourceStream = assembly.GetManifestResourceStream(resourceName);
            if (resourceStream == null)
                throw new Exception("Resource stream is null. Please contact with devs.");
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                using (StreamWriter sw = File.CreateText(Path.Combine("Config", filename)))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        sw.WriteLine(line);
                    }
                    sw.Close();
                }
                reader.Close();
            }
        }
        /// <summary>
        /// Asks user yes or no.
        /// User need to be in usermode (-u).
        /// </summary>
        /// <param name="question">Question to ask</param>
        /// <returns>yes - true<br></br>no - false</returns>
        private bool Ask(string question)
        {
            if (!Program.userMode)
                return false;
            Console.Write(question + " (y/n) : ");
            ConsoleKeyInfo key;
            while (true)
            {
                key = Console.ReadKey();
                if (key.Key == ConsoleKey.Y || key.Key == ConsoleKey.N)
                    break;
                Console.Write("\nInvalid option. (y/n) : ");
            }
            Console.WriteLine();
            return key.Key == ConsoleKey.Y;
        }

        private bool configExists(string filename)
        {
            return File.Exists(Path.Combine("Config", filename));
        }
    }
}
