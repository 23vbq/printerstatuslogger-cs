using PrinterStatusLogger.PrinterManaging;
using System.Net;
using System.Net.Mail;
using System.Text;
using Windows.Security.Credentials;

namespace PrinterStatusLogger
{
    public static class Alerter
    {
        public struct AlertTonerLevelPrinterObj
        {
            public string Name;
            public int TonerLevel;

            public AlertTonerLevelPrinterObj(string name, int tonerLevel)
            {
                Name = name;
                TonerLevel = tonerLevel;
            }
        }
        public struct AlertErrorPrinterObj
        {
            public string Name;
            public string Address;
            public string CausedBy;

            public AlertErrorPrinterObj(string name, string address, string causedby)
            {
                Name = name;
                Address = address;
                CausedBy = causedby;
            }
        }

        /*
         * Smtp Server
         */
        public static string SmtpServer = "";
        public static int SmtpPort = -1;
        public static string MessageRecipients = "";

        /*
         * Rules
         */ 
        public static int minTonerLevel = -1;
        // Default values
        private const int _DEF_minTonerLevel = 20;

        /*
         * Properties
         */
        private static bool Initialized = false;
        private static SmtpClient _smtpClient;
        private static NetworkCredential _credential;
        private static bool _errorBit;

        /*
         * Buffer
         */
        private static List<AlertTonerLevelPrinterObj> _alertTonerLevelBuffer;
        private static List<Printer> _alertUnavaliableWebInterfaceBuffer;
        private static List<AlertErrorPrinterObj> _alertErrorBuffer;

        static Alerter()
        {
            _smtpClient = new SmtpClient(); // TODO Server in config, better config itd.
            _smtpClient.EnableSsl = true;
            _smtpClient.UseDefaultCredentials = false;
            _credential = new NetworkCredential();
            _errorBit = false;

            _alertTonerLevelBuffer = new List<AlertTonerLevelPrinterObj>();
            _alertUnavaliableWebInterfaceBuffer = new List<Printer>();
            _alertErrorBuffer = new List<AlertErrorPrinterObj>();
        }
        public static void Initialize(PasswordCredential pc, Action loadAlerterConfig)
        {
            if (Program.noAlertMode)
                return;
            if (pc == null)
            {
                Logger.Log(LogType.ERROR, "Cannot initialize Alerter: Credentials is null");
                return;
            }
            pc.RetrievePassword();
            _credential = new NetworkCredential(pc.UserName, pc.Password);
            _smtpClient.Credentials = _credential;

            loadAlerterConfig.Invoke();
            Initialized = (InitializeSmtpServer() && InitializeRules());
        }
        private static bool InitializeSmtpServer()
        {
            string hex = Logger.BitCheck(new bool[]
            {
                SmtpServer == "",
                SmtpPort == -1,
                MessageRecipients == ""
            }, 1);
            if (hex != "0x00")
            {
                Logger.Log(LogType.ERROR, "Cannot initialize Alerter SmtpServer: Check code - " + hex);
                return false;
            }
            _smtpClient.Host = SmtpServer;
            _smtpClient.Port = SmtpPort;
            return true;
        }
        private static bool InitializeRules()
        {
            if (minTonerLevel == -1)
            {
                Logger.Log(LogType.WARNING, "Alerter Rules: minTonerLevel is not set, default value [20] will be used");
                minTonerLevel = _DEF_minTonerLevel;
            }
            return true;
        }

        public static void Handler(Printer printer, int tonerLevel, bool avalible)
        {
            if (Program.noAlertMode)
                return;
            if (!avalible)
            {
                _alertUnavaliableWebInterfaceBuffer.Add(printer);
                return;
            }
            if (tonerLevel <= minTonerLevel)
                _alertTonerLevelBuffer.Add(new AlertTonerLevelPrinterObj(printer.Name, tonerLevel));
        }
        public static void AddError(Printer printer, string causedby)
        {
            _alertErrorBuffer.Add(new AlertErrorPrinterObj(printer.Name, printer.Address, causedby));
        }

        public static void Send()
        {
            if (Program.noAlertMode)
                return;
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
            // Initializing message
            MailMessage message = new MailMessage();
            message.From = new MailAddress(_credential.UserName);
            message.To.Add(MessageRecipients);
            message.IsBodyHtml = true;
            message.Subject = "PrinterStatusLogger " + DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
            // Building message
            StringBuilder sb = new StringBuilder();
            bool isGood = true;
            bool newline = false;
            if (_alertTonerLevelBuffer.Count != 0)
            {
                AddPrinterTonerAlert(sb);
                isGood = false;
                newline = true;
            }
            if (_alertUnavaliableWebInterfaceBuffer.Count != 0)
            {
                if (newline) sb.Append("<br>");
                AddUnavalibleWebInterfaceAlert(sb);
                isGood = false;
                newline = true;
            }
            if (_alertErrorBuffer.Count != 0)
            {
                if (newline) sb.Append("<br>");
                AddErrorAlert(sb);
                isGood = false;
                newline = true;
            }
            if (isGood)
                sb.Append("Every printer working fine! :)");
            message.Body = sb.ToString();
            // Sending message
            try
            {
                _smtpClient.Send(message);
                Logger.Log(LogType.INFO, "Alert was sent");
                // Clearing
                _alertTonerLevelBuffer.Clear();
                _alertUnavaliableWebInterfaceBuffer.Clear();
                sb.Clear();
            } catch (SmtpException ex)
            {
                Logger.Log(LogType.ERROR, ex.Message);
                _errorBit = true; // To prevent trying to send in existing process if once failed, maybe for future implementations (or not :D)
            }
        }
        /*
         * Alert Build
         */
        private static void AddPrinterTonerAlert(StringBuilder sb)
        {
            sb.Append("<b>Low toner level: </b><br>");
            sb.Append("<table>");
            sb.Append("<tr><th>Name</th><th>Toner Level</th></tr>");
            foreach (var x in _alertTonerLevelBuffer)
                sb.Append("<tr><td>" + x.Name + "</td><td>" + x.TonerLevel + "%</td></tr>");
            sb.Append("</table>");
        }
        private static void AddUnavalibleWebInterfaceAlert(StringBuilder sb)
        {
            sb.Append("<b>Unavaliable web interface: </b><br>");
            sb.Append("<table>");
            sb.Append("<tr><th>Name</th><th>Address</th></tr>");
            foreach (var x in _alertUnavaliableWebInterfaceBuffer)
                sb.Append("<tr><td>" + x.Name + "</td><td>" + x.Address + "</td></tr>");
            sb.Append("</table>");
        }
        private static void AddErrorAlert(StringBuilder sb)
        {
            sb.Append("<b>Error when scanning: </b><br>");
            sb.Append("<table>");
            sb.Append("<tr><th>Name</th><th>Address</th><th>Caused by</th></tr>");
            foreach (var x in _alertErrorBuffer)
                sb.Append("<tr><td>" + x.Name + "</td><td>" + x.Address + "</td><td>" + x.CausedBy + "</td></tr>");
            sb.Append("</table>");
        }
    }
}
