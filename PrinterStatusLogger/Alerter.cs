using PrinterStatusLogger.PrinterManaging;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace PrinterStatusLogger
{
    public static class Alerter
    {
        public struct AlertPrinterObj
        {
            public string Address;
            public int TonerLevel;

            public AlertPrinterObj(string address, int tonerLevel)
            {
                Address = address;
                TonerLevel = tonerLevel;
            }
        }

        public static readonly int minTonerLevel = 90;

        private static SmtpClient _smtpClient;
        private static NetworkCredential _credential;
        private static bool _errorBit;

        private static List<AlertPrinterObj> _alertBuffer;

        static Alerter()
        {
            _smtpClient = new SmtpClient("smtp.gmail.com", 587);
            _smtpClient.EnableSsl = true;
            _credential = new NetworkCredential("***REMOVED***", "***REMOVED***"); //***REMOVED***
            _smtpClient.UseDefaultCredentials = false;
            _smtpClient.Credentials = _credential;
            _errorBit = false;

            _alertBuffer = new List<AlertPrinterObj>();
        }

        public static void Handler(Printer printer, int tonerLevel)
        {
            if (tonerLevel > minTonerLevel)
                return;
            _alertBuffer.Add(new AlertPrinterObj(printer.Address, tonerLevel));
        }

        public static void Send()
        {
            if (_errorBit)
            {
                Logger.Log(LogType.WARNING, "Alerter error bit is set, skipping sending...");
            }
            MailMessage message = new MailMessage();
            message.From = new MailAddress("***REMOVED***");
            message.To.Add("***REMOVED***");
            message.IsBodyHtml = true;
            message.Subject = "PrinterStatusLogger " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            StringBuilder sb = new StringBuilder();
            AddPrinterTonerAlert(sb);
            message.Body = sb.ToString();
            try
            {
                _smtpClient.Send(message);
                Logger.Log(LogType.INFO, "Alert was sent");
                _alertBuffer.Clear();
                sb.Clear();
            } catch (SmtpException ex)
            {
                Logger.Log(LogType.ERROR, ex.Message);
                _errorBit = true;
            }
        }
        private static void AddPrinterTonerAlert(StringBuilder sb)
        {
            sb.Append("<b>Low toner level: </b><br>");
            foreach (var x in _alertBuffer)
            {
                sb.Append("&nbsp;&nbsp;" + Logger.BuildLog(LogType.PRNT_INFO, x.Address + " Toner: " + x.TonerLevel) + "<br>");
            }
        }
    }
}
