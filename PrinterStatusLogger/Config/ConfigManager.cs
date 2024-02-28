using PrinterStatusLogger.PrinterManaging;
using PrinterStatusLogger.Logging;
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
            if (!ConfigExists(_printersConfigFilename)){
                Logger.Log(LogType.WARNING, "Printers config not found");
                if (Ask("Do you want to create default config file?"))
                {
                    CreateConfigFile(_printersConfigFilename, _DEF_printersConfigResourceName);
                    Logger.Log(LogType.INFO, "File " + _printersConfigFilename + " created at " + Path.GetFullPath(Path.Combine("Config", _printersConfigFilename)));
                }
                else
                    throw new Exception("Config file not found");
            }
            ReadConfigOld(path, (line) =>
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
            if (!ConfigExists(_alerterConfigFilename))
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
            Dictionary<string, string> config = ReadConfig(path);
            // Load required properties
            GetConfigProperty<string>(config, "server", out Alerter.SmtpServer);
            GetConfigProperty<ushort>(config, "port", out Alerter.SmtpPort);
            GetConfigProperty<string>(config, "recipients", out Alerter.MessageRecipients);
            // Load not required properties
            if (!GetConfigProperty<int>(config, "minTonerLevel", out Alerter.R_minTonerLevel))
                Alerter.R_minTonerLevel = -1;
            if (GetConfigProperty<string>(config, "unavaliablePrinters", out string? unavaliablePrinters))
                Alerter.R_unavaliablePrinters = (unavaliablePrinters == "on" ? true : (unavaliablePrinters == "off" ? false : throw new Exception("Invalid data in config on key: unavaliablePrinters"))); // TODO Need to throw line
            if (GetConfigProperty<string>(config, "scanErrors", out string? scanErrors))
                Alerter.R_scanErrors = (scanErrors == "on" ? true : (scanErrors == "off" ? false : throw new Exception("Invalid data in config on key: scanErrors")));
        }
        /*
         * Loading models
         */
        public void LoadPrinterModels(Action<PrinterModel> registerModel)
        {
            string[] files;
            try
            {
                files = GetPrinterModelConfigFiles();
            }
            catch (Exception ex) // FIXME is ex needed
            {
                throw;
            }
            int n = 0;
            foreach (string file in files)
            {
                if (!file.EndsWith(".cfg"))
                {
                    Logger.Log(LogType.WARNING, "Skipping (not .cfg) file: " + file);
                    continue;
                }
                Dictionary<string, string> config = ReadConfig(file);
                string? id, name, readtonerlevelpath, readtonerlevelregex;
                // Load properties
                GetConfigProperty<string>(config, "id", out id);
                GetConfigProperty<string>(config, "name", out name);
                GetConfigProperty<string>(config, "readtonerlevelpath", out readtonerlevelpath);
                GetConfigProperty<string>(config, "readtonerlevelregex", out readtonerlevelregex);
                string hex = Logger.BitCheck(new bool[]
                {
                    id == null,
                    name == null,
                    readtonerlevelpath == null,
                    readtonerlevelregex == null
                }, 1);
                if (hex != "0x00")
                {
                    Logger.Log(LogType.ERROR, "Not all arguments specified for model " + file + ": Check code - " + hex);
                    continue;
                }
#pragma warning disable CS8604 // Cannot be null
                registerModel.Invoke(new PrinterModel(id, name, readtonerlevelpath, readtonerlevelregex));
#pragma warning restore CS8604
                n++;
            }
            Logger.Log(LogType.INFO, "Loaded printer models: " + n);
        }
        /// <summary>
        /// Lists model config files
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">If directory not exists or is empty</exception>
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
        /// <summary>
        /// Returns credentials for Alerter
        /// </summary>
        /// <returns>PasswordCredential or null when noAlertMode</returns>
        /// <exception cref="Exception">If credentials doesn't exists</exception>
        public PasswordCredential GetAlerterCreds()
        {
            if (Program.noAlertMode)
#pragma warning disable CS8603
                return null;
#pragma warning restore CS8603
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
            // FIXME need testing if can be null
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
        private void ReadConfigOld(string path, Func<string, bool> function)
        {
            Logger.Log(LogType.INFO, "Reading config file: " + path);
            string line;
            int loaded = 0;
            using (StreamReader sr = new StreamReader(path))
            {
                int n = 0;
#pragma warning disable CS8600
                while ((line = sr.ReadLine()) != null)
#pragma warning restore CS8600
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
        /*
         * IN PROGRESS
         */
        /// <summary>
        /// Reads config file. Gets one property each line in format <i>key = value</i>.<br></br>
        /// All text after # is ignored.<br></br>
        /// You can easily get desired property with GetConfigProperty
        /// </summary>
        /// <param name="path">Path to config file</param>
        /// <returns>Dictionary of keys and values in <b>string</b> format</returns>
        private Dictionary<string, string> ReadConfig(string path)
        {
            Logger.Log(LogType.INFO, "Reading config file: " + path);
            string line;
            string[] optline;
            Dictionary<string, string> result = new Dictionary<string, string>();
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
                    line = line.Split('#', 2)[0];
                    optline = line.Split('=', 2);
                    if (optline.Length != 2)
                    {
                        Logger.Log(LogType.WARNING, "Invalid data encountered in " + path + " at line " + n);
                        continue;
                    }
                    result.Add(optline[0].Trim(), optline[1].Trim());
                }
            }
            Logger.Log(LogType.INFO, "Loaded properties from config: " + result.Count);
            return result;
        }
        /// <summary>
        /// Gets required property from <i>ReadConfig</i> result and converts to desired <b>T</b> type.
        /// </summary>
        /// <typeparam name="T">Desired value type</typeparam>
        /// <param name="config">Dictionary to find in</param>
        /// <param name="key">Name of property to find</param>
        /// <param name="output">Variable to store value of property</param>
        /// <returns><b>True</b> if everything fine <br></br><b>False</b> if not found, cannot convert to type or config is null</returns>
        private bool GetConfigProperty<T>(Dictionary<string, string> config, string key, out T? output)
        {
            output = default(T);
            if (config == null)
                return false;
            if (!config.ContainsKey(key))
            {
                Logger.Log(LogType.V_WARNING, "Property " + key + " not found in config!");
                return false;
            }
            try
            {
                output = (T)Convert.ChangeType(config[key], typeof(T));
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.ERROR, "Cannot parse property " + key + ":" + ex.Message);
                return false;
            }
            return true;
        }
        /*
         * END OF IN PROGRESS
         */
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

        private bool ConfigExists(string filename)
        {
            return File.Exists(Path.Combine("Config", filename));
        }
    }
}
