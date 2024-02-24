using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace PrinterStatusLogger.PrinterManaging
{
    public class Printer
    {
        public string Name;
        public string Address { get; private set; }
        public PrinterModel Model { get; private set; }
        public bool avaliable { get; private set; } = true; // By default is true

        public Printer(string name, string address, PrinterModel model)
        {
            Name = name;
            Address = address;
            Model = model;
        }

        public int GetTonerLevel()
        {
            if (Model == null)
                throw new Exception("Model of " + Name + " is null");
            string? content = null;
            try
            {
                content = GetPrinterWebInterface();
            } catch (Exception ex)
            {
                Ping();
                throw;
            }
            if (content == null)
            {
                Logger.Log(LogType.ERROR, "Something gone wrong with " + Name + " at " + Address);
                throw new Exception("GetTonerLevel exited with an error");
            }
            return Model.ReadTonerLevelFromResponse(content);
        }
        public bool Ping()
        {
            Logger.Log(LogType.V_INFO, "Pinging " + Address);
            Ping p = new Ping();
            string addr = Regex.Replace(Address, @"(https://)|(http://)", "");
            addr = addr.Substring(0, addr.Length - 1);
            PingReply reply = p.Send(addr);
            avaliable = reply.Status == IPStatus.Success;
            Logger.Log(LogType.WARNING, "Ping of " +  Address + " " + (avaliable ? "successful" : "failed"));
            return avaliable; // TODO do not works properly, and consider how to implement
        }
        private string? GetPrinterWebInterface()
        {
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
        public UInt16 GetPort()
        {
            Match m = Regex.Match(this.Address, @"(?<=:)[0-9]{1,5}");
            if (m.Success)
                return UInt16.Parse(m.Value);
            else // It is very not optimized - bad
            {
                Logger.Log(LogType.V_INFO, "GetPort returns well known protocol port");
                if (this.Address.StartsWith("http://"))
                    return 80;
                else if (this.Address.StartsWith("https://"))
                    return 443;
                else
                {
                    throw new Exception("Port is not known");
                }
            }
        }
    }
}
