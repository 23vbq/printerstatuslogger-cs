using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace PrinterStatusLogger.PrinterManaging
{
    public class Printer
    {
        public string Name;
        public string Address { get; private set; }
        public PrinterModel Model { get; private set; }
        public PrinterAvailability avaliable { get; private set; } = new PrinterAvailability { Icmp = true, PortOpen = true}; // By default is true

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
            PingReply reply = p.Send(GetIP());
            avaliable.Icmp = reply.Status == IPStatus.Success;
            Logger.Log(LogType.WARNING, "Ping of " +  Address + " " + (avaliable.Icmp ? "successful" : "failed"));
            return avaliable.Icmp; // TODO do not works properly, and consider how to implement
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
        public bool IsPortOpen()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(GetIP(), GetPort(), null, null);
                    var success = result.AsyncWaitHandle.WaitOne(Program.s_connectionTimeout);
                    client.EndConnect(result);
                    avaliable.PortOpen = success;
                    return avaliable.PortOpen;
                }
            } catch
            {
                avaliable.PortOpen = false;
                return avaliable.PortOpen;
            }
        }
        public string GetIP()
        {
            string result = Regex.Replace(this.Address, @"(https://)|(http://)", "");
            if (result.EndsWith('/'))
                result = result.Substring(0, result.Length - 1);
            return result;
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
