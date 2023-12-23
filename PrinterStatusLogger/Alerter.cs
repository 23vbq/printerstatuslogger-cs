using System.Net;
using System.Net.Mail;

namespace PrinterStatusLogger
{
    public static class Alerter
    {
        private static int minTonerLevel;

        private static SmtpClient _smtpClient;
        private static NetworkCredential _credential;

        static Alerter()
        {
            _smtpClient = new SmtpClient();
            _credential = new NetworkCredential("***REMOVED***", "***REMOVED***", "smtp.gmail.com");
            _smtpClient.UseDefaultCredentials = false;
            _smtpClient.Credentials = _credential;
            _smtpClient.Host = "smtp.gmail.com";
        }
    }
}
