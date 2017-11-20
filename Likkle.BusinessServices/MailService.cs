using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Likkle.BusinessServices
{
    public class MailService : IMailService
    {
        private SmtpClient _smtpClient;
        private readonly IConfigurationWrapper _configuration;

        public MailService(IConfigurationWrapper configuration)
        {
            this._configuration = configuration;
        }

        public async Task SendEmailForThrownException(string recipientEmail, string exceptionBody)
        {
            _smtpClient = new SmtpClient()
            {
                Host = this._configuration.SmtpClientHost,
                Port = 587,
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(this._configuration.SupportEmail, this._configuration.SupportEmailPassword)
            };

            var mailMessage = new MailMessage()
            {
                From = new MailAddress(this._configuration.SupportEmail, "Boongaloo"),
                To = { recipientEmail },
                Subject = $"Exception was thrown in Boongaloo ({this._configuration.HostingEnvironment})",
                Body = $"{exceptionBody}",
                BodyEncoding = Encoding.UTF8
            };

            await _smtpClient.SendMailAsync(mailMessage);
        }
    }
}
