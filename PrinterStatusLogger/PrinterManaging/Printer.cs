using PrinterStatusLogger.Logging;
using System.Text.RegularExpressions;

namespace PrinterStatusLogger.PrinterManaging
{
    public class Printer
    {
        public string Name;
        public string Address { get; private set; }
        public PrinterModel Model { get; private set; }
        public PrinterAvailability Availability { get; private set; }

        public Printer(string name, string address, PrinterModel model)
        {
            this.Name = name;
            this.Address = address;
            this.Model = model;

            Availability = new PrinterAvailability(GetIP(this.Address), GetPort(this.Address));
        }

        public int GetTonerLevel()
        {
            if (Model == null)
                throw new Exception("Model of " + Name + " is null");
            string? content = null;
            try
            {
                content = GetPrinterWebInterface();
            } catch
            {
                throw;
            }
            if (content == null)
            {
                Logger.Log(LogType.ERROR, "Something gone wrong with " + Name + " at " + Address);
                throw new Exception("GetTonerLevel exited with an error");
            }
            return Model.ReadTonerLevelFromResponse(content);
        }
        private string? GetPrinterWebInterface()
        {
            Logger.Log(LogType.V_INFO, "Checking for open port " + Address);
            string? content = null;
            using (var client = new HttpClient())
            {
                var result = client.GetAsync(this.Address + Model.ReadTonerLevelPath);
                result.Wait();
                if (result.Result.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception(result.Result.StatusCode.ToString());
                content = result.Result.Content.ReadAsStringAsync().Result;
            }
            return content;
        }
        public static string GetIP(string address)
        {
            string result = Regex.Replace(address, @"(https://)|(http://)", "");
            if (result.EndsWith('/'))
                result = result.Substring(0, result.Length - 1);
            return result;
        }
        public static UInt16 GetPort(string address)
        {
            Match m = Regex.Match(address, @"(?<=:)[0-9]{1,5}");
            if (m.Success)
                return UInt16.Parse(m.Value);
            else // It is very not optimized - bad
            {
                Logger.Log(LogType.V_INFO, "GetPort(" + address + ") returns well known protocol port");
                if (address.StartsWith("http://"))
                    return 80;
                else if (address.StartsWith("https://"))
                    return 443;
                else
                {
                    throw new Exception("Port is not known");
                }
            }
        }
    }
}
