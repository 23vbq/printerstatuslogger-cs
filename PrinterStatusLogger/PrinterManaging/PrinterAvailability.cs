using PrinterStatusLogger.Logging;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PrinterStatusLogger.PrinterManaging
{
    public class PrinterAvailability
    {
        private bool? _icmp;
        public bool Icmp
        {
            get
            {
                if (_icmp == null)
                {
                    Logger.Log(LogType.V_WARNING, "Icmp is null. Requesting ping");
                    return Ping();
                }
                return (bool)_icmp;
            }
        }
        private bool? _portOpen;
        public bool PortOpen
        {
            get
            {
                if (_portOpen == null)
                {
                    Logger.Log(LogType.V_WARNING, "PortOpen is null. Requesting check");
                    return CheckPortOpen();
                }
                return (bool)_portOpen;
            }
        }

        private string _address;
        private UInt16 _port;
        private bool _checked;

        public PrinterAvailability(string address, UInt16 port)
        {
            this._address = address;
            this._port = port;
        }

        public static bool operator true(PrinterAvailability x) => x.Icmp && x.PortOpen;
        public static bool operator false(PrinterAvailability x) => !x.Icmp || !x.PortOpen;
        public static bool operator !(PrinterAvailability x) => !x.Icmp || !x.PortOpen;

        public static string WriteAvailabilityStatus(PrinterAvailability x)
        {
            string result = "";
            result += "Ping " + (x.Icmp ? "successful" : "failed") + " : ";
            result += "Port is " + (x.PortOpen ? "open" : "closed");
            return result;
        }

        public bool Ping()
        {
            Logger.Log(LogType.V_INFO, "Pinging " + this._address);
            Ping p = new Ping();
            PingReply reply = p.Send(_address);
            this._icmp = reply.Status == IPStatus.Success;
            Logger.Log(LogType.WARNING, "Ping of " + this._address + " " + (this.Icmp ? "successful" : "failed"));
            return this.Icmp; // TODO do not works properly, and consider how to implement
        }
        public bool CheckPortOpen()
        {
            Logger.Log(LogType.V_INFO, "Checking is port open at " + this._address + ":" + this._port);
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(this._address, this._port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(Program.s_connectionTimeout);
                    client.EndConnect(result);
                    this._portOpen = success;
                    Logger.Log(LogType.WARNING, "Port " + this._address + ":" + this._port + " is " + (this.Icmp ? "open" : "closed"));
                    return this.PortOpen;
                }
            }
            catch
            {
                this._portOpen = false;
                Logger.Log(LogType.WARNING, "Port " + this._address + ":" + this._port + " is closed");
                return this.PortOpen;
            }
        }
    }
}
