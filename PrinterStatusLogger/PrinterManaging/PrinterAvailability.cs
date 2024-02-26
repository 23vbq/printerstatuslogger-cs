namespace PrinterStatusLogger.PrinterManaging
{
    public class PrinterAvailability
    {
        public bool Icmp;
        public bool PortOpen;

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
    }
}
