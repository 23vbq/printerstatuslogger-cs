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
                //Logger.Log(LogType.ERROR, ex.Message + " at " + Address);
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
    }
}
