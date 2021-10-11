using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EmailService
{
    public class EmailRequestBo<T> where T : new()
    {
        public string ApplicationName { get; set; }
        public string MailFor { get; set; }
        public string Environment { get; set; }
        public T Data { get; set; }

    }

    public class CreateQuoteEmailBo
    {
        public string QuoteId { get; set; }
        public string EffectiveDate { get; set; }
        public string ExpirationDate { get; set; }
    }

    public class QuoteExpirationBo
    {
        public string QuoteId { get; set; }
        public string ExpirationDate { get; set; }
    }

    public interface ITemplateProvider
    {
        string GenerateTemplate();
    }

    public interface IEmailServiceProvider
    {
        void PostMail(string msg);
    }

    public interface IEmailService
    {
        void Send(string msg);
    }

    public class SmtpEmailProvider : IEmailServiceProvider
    {
        private readonly SmtpClient client = null;

        public void PostMail(string msg)
        {
            this.client.Send(msg);
        }
        public SmtpEmailProvider()
        {
            this.client = new SmtpClient("", "", "");
        }
        private class SmtpClient
        {
            public SmtpClient(string exchangeName, string userName, string password)
            {

            }

            public void Send(string message)
            {
                Console.WriteLine($"Email sent using SMTP: {message}");
            }
        }
    }
    public class SendGridEmailProvider : IEmailServiceProvider
    {
        private readonly SendGridClient client;

        public SendGridEmailProvider(string connectionString)
        {
            this.client = new SendGridClient(connectionString);
        }
        class SendGridClient
        {

            public SendGridClient(string connectionString)
            {

            }

            public void Send(string message)
            {
                Console.Write($"Mail sent using SendGrid client {message}");
            }
        }
        public void PostMail(string msg)
        {
            this.client.Send(msg);
        }
    }

    public class CreateQuoteTemplateProvider : ITemplateProvider
    {
        private readonly CreateQuoteEmailBo source;

        public CreateQuoteTemplateProvider(string data)
        {
            source = JsonConvert.DeserializeObject<CreateQuoteEmailBo>(data);
        }
        public string GenerateTemplate()
        {
            return $"your quoute is genearted with effect from {source.EffectiveDate} and will be valid till {source.ExpirationDate}";
        }
    }

    public class QuoteExpirationTemplateProvider : ITemplateProvider
    {
        private readonly QuoteExpirationBo source;

        public QuoteExpirationTemplateProvider(string data)
        {
            source = JsonConvert.DeserializeObject<QuoteExpirationBo>(data);
        }

        public string GenerateTemplate()
        {
            return $"Please note your quote has expired on {source.ExpirationDate}";
        }

    }

    public class EmailService : IEmailService
    {
        private readonly IEmailServiceProvider emailServiceProvider;

        public EmailService(IEmailServiceProvider emailServiceProvider)
        {
            this.emailServiceProvider = emailServiceProvider;
        }
        public void Send(string msg)
        {
            this.emailServiceProvider.PostMail(msg);
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            string messageData = "";
            ProcessQueueMessage(messageData);
        }

        static void ProcessQueueMessage(string message)
        {
            /*Check message to identify the mail type
             * Deserialize to correct entity
             * Prepare template with data
             * Send email
             */
            string createQuoteMsg = JsonConvert.SerializeObject(new CreateQuoteEmailBo
            {
                EffectiveDate = Convert.ToString(DateTime.Today),
                ExpirationDate = Convert.ToString(DateTime.Today.AddYears(1)),
                QuoteId = "3432434"
            });
            SendCreateQuoteEmail(createQuoteMsg);

            var quoteExpirationMsg = JsonConvert.SerializeObject(new QuoteExpirationBo { ExpirationDate = Convert.ToString(DateTime.Today), QuoteId = "7868678678" });
            SendQuoteExpirationEmail(quoteExpirationMsg);

        }

        static void SendCreateQuoteEmail(string message)
        {
            IEmailServiceProvider serviceProvider = new SmtpEmailProvider();
            IEmailService mailService = new EmailService(serviceProvider);
            ITemplateProvider templateProvider = new CreateQuoteTemplateProvider(message);
            var mailBody = templateProvider.GenerateTemplate();
            mailService.Send(mailBody);
        }

        static void SendQuoteExpirationEmail(string message)
        {
            IEmailServiceProvider serviceProvider = new SendGridEmailProvider("");
            IEmailService mailService = new EmailService(serviceProvider);
            ITemplateProvider templateProvider = new QuoteExpirationTemplateProvider(message);
            var mailBody = templateProvider.GenerateTemplate();
            mailService.Send(mailBody);
        }
    }
}
