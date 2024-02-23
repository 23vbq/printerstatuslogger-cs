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
        public static ushort SmtpPort = 0; // Port is ushort => 0 - 65535 / 16 bits
        public static string MessageRecipients = "";

        /*
         * Rules
         */ 
        public static int R_minTonerLevel = -1;
        public static bool? R_unavaliablePrinters = null;
        public static bool? R_scanErrors = null;
        // Default values
        private const int _DEF_R_minTonerLevel = 20;
        private const bool _DEF_R_unavaliablePrinters = true;
        private const bool _DEF_R_scanErrors = true;

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
                SmtpPort == 0,
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
            if (R_minTonerLevel == -1)
            {
                Logger.Log(LogType.WARNING, "Alerter Rules: minTonerLevel is not set, default value [" + _DEF_R_minTonerLevel + "] will be used");
                R_minTonerLevel = _DEF_R_minTonerLevel;
            }
            if (R_unavaliablePrinters == null)
            {
                Logger.Log(LogType.WARNING, "Alerter Rules: unavaliablePrinters is not set, default value [" + _DEF_R_unavaliablePrinters.ToString() + "] will be used");
                R_minTonerLevel = _DEF_R_minTonerLevel;
            }
            if (R_scanErrors == null)
            {
                Logger.Log(LogType.WARNING, "Alerter Rules: scanErrors is not set, default value [" + _DEF_R_scanErrors.ToString() + "] will be used");
                R_minTonerLevel = _DEF_R_minTonerLevel;
            }
            return true;
        }

        public static void Handler(Printer printer, int tonerLevel)
        {
            if (Program.noAlertMode)
                return;
            if (tonerLevel < 0)
            {
                if (printer.avaliable)
                    AddError(printer, "Ping failed"); // FIXME is this needed here?
                else
                    _alertUnavaliableWebInterfaceBuffer.Add(printer);
                return;
            }
            if (tonerLevel <= R_minTonerLevel)
                _alertTonerLevelBuffer.Add(new AlertTonerLevelPrinterObj(printer.Name, tonerLevel));
        }
        public static void AddError(Printer printer, string causedby)
        {
            _alertErrorBuffer.Add(new AlertErrorPrinterObj(printer.Name, printer.Address, causedby + " | " + (printer.avaliable ? "Ping successful" : "Ping failed")));
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
                //AddPrinterTonerAlert(sb);
                AddAlertTable<AlertTonerLevelPrinterObj>(sb, "Low toner level:", new string[] { "Name", "Toner Level" }, _alertTonerLevelBuffer, (x) =>
                {
                    return "<td> " + x.Name + " </td><td> " + x.TonerLevel + "%</td>";
                });
                isGood = false;
                newline = true;
            }
            if (_alertUnavaliableWebInterfaceBuffer.Count != 0)
            {
                if (newline) sb.Append("<br>");
                //AddUnavalibleWebInterfaceAlert(sb);
                AddAlertTable<Printer>(sb, "Unavaliable web interface:", new string[] { "Name", "Address" }, _alertUnavaliableWebInterfaceBuffer, (x) =>
                {
                    return "<td> " + x.Name + " </td><td> " + x.Address + "</td>";
                });
                isGood = false;
                newline = true;
            }
            if (_alertErrorBuffer.Count != 0)
            {
                if (newline) sb.Append("<br>");
                AddAlertTable<AlertErrorPrinterObj>(sb, "Error when scanning:", new string[] { "Name", "Address", "Caused by" }, _alertErrorBuffer, (x) =>
                {
                    return "<td> " + x.Name + " </td><td> " + x.Address + "</td><td>" + x.CausedBy + "</td>";
                });
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
        /// <summary>
        /// Creates table in alert message from provided data list.
        /// </summary>
        /// <typeparam name="T">Type of data</typeparam>
        /// <param name="sb">Message string builder in which table will be created</param>
        /// <param name="title">Title of table</param>
        /// <param name="headers">Headers of table</param>
        /// <param name="alertList">Data list of alerts</param>
        /// <param name="rowBuilder">Lambda function to combine properties in one row<br></br>Each column should be in HTML td tag</param>
        private static void AddAlertTable<T>(StringBuilder sb, string title, string[] headers, List<T> dataList, Func<T, string> rowBuilder)
        {
            // Prepare table
            sb.Append("<b>" + title + "</b><br>");
            sb.Append("<table><tr>");
            // Build headers row
            foreach (string header in headers)
                sb.Append("<th>" + header + "</th>");
            sb.Append("</tr>");
            // Build rows
            foreach (T x in dataList)
                sb.Append("<tr>" + rowBuilder.Invoke(x) + "</tr>");
            // Close table
            sb.Append("</table>");
        }
    }
}
