using PrinterStatusLogger.Config;
using PrinterStatusLogger.PrinterManaging;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;
using Windows.Security.Credentials;

namespace PrinterStatusLogger
{
    public static class Alerter
    {
        public struct AlertPrinterObj
        {
            public string Name;
            public int TonerLevel;

            public AlertPrinterObj(string name, int tonerLevel)
            {
                Name = name;
                TonerLevel = tonerLevel;
            }
        }

        public static string SmtpServer = "";
        public static int SmtpPort = -1;
        public static string MessageRecipients = "";

        public static readonly int minTonerLevel = 90;

        private static bool Initialized = false;
        private static SmtpClient _smtpClient;
        private static NetworkCredential _credential;
        private static bool _errorBit;

        private static List<AlertPrinterObj> _alertBuffer;

        static Alerter()
        {
            _smtpClient = new SmtpClient("smtp.gmail.com", 587); // TODO Server in config, better config itd.
            _smtpClient.EnableSsl = true;
            _smtpClient.UseDefaultCredentials = false;
            _credential = new NetworkCredential();
            _errorBit = false;

            _alertBuffer = new List<AlertPrinterObj>();
        }
        public static void Initialize(PasswordCredential pc, Action loadAlerterConfig)
        {
            if (pc == null)
            {
                Logger.Log(LogType.ERROR, "Cannot initialize Alerter: Credentials is null");
                return;
            }
            pc.RetrievePassword();
            _credential = new NetworkCredential(pc.UserName, pc.Password);
            _smtpClient.Credentials = _credential;

            loadAlerterConfig.Invoke();
            // Debug Info: Here is reversed, so 0x04 means {false, true, true}
            BitArray settingcheck = new BitArray(
            new bool[]{
                SmtpServer != "",
                SmtpPort != -1,
                MessageRecipients != ""
            });
            byte[] code = new byte[1];
            settingcheck.CopyTo(code, 0);
            //settingcheck.Not(); not working
            string hex = BitConverter.ToString(code);
            Initialized = hex == "07";
            if (!Initialized)
            {
                Logger.Log(LogType.ERROR, "Cannot initialize Alerter: Check code - 0x" + hex);
            }
        }

        public static void Handler(Printer printer, int tonerLevel)
        {
            if (tonerLevel > minTonerLevel)
                return;
            _alertBuffer.Add(new AlertPrinterObj(printer.Name, tonerLevel));
        }

        public static void Send()
        {
            if (!Initialized)
            {
                Logger.Log(LogType.ERROR, "Alerter not initialized");
                return;
            }
            if (_errorBit)
            {
                Logger.Log(LogType.WARNING, "Alerter error bit is set, skipping sending...");
                return;
            }
            MailMessage message = new MailMessage();
            message.From = new MailAddress(_credential.UserName);
            message.To.Add(MessageRecipients);
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
            sb.Append("<table>");
            sb.Append("<tr><th>Name</th><th>Toner Level</th></tr>");
            foreach (var x in _alertBuffer)
            {
                //sb.Append("&nbsp;&nbsp;" + Logger.BuildLog(LogType.PRNT_INFO, x.Name + " Toner: " + x.TonerLevel) + "<br>");
                //sb.Append("&nbsp;&nbsp;" + x.Name + "&emps;" + "<br>");
                sb.Append("<tr><td>" + x.Name + "</td><td>" + x.TonerLevel + "%</td></tr>");
            }
            sb.Append("</table>");
        }
    }
}
