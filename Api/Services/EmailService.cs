using FluentEmail.Core;

namespace Api.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IFluentEmailFactory _fleuntEmailfactory;

        public EmailService(IFluentEmailFactory fleuntEmailfactory,ILogger<EmailService> logger)
        {
            _logger = logger;
            _fleuntEmailfactory = fleuntEmailfactory;
        }
        public async Task Send(EmailMessageModel emailMessage, Boolean isHtml=true)
        {
            _logger.LogInformation("Sending email");
            await _fleuntEmailfactory.Create().To(emailMessage.ToAddress)
                .Subject(emailMessage.Subject)
                .Body(emailMessage.Body,isHtml)
                .SendAsync();
        }
    }

    public interface IEmailService
    {
        Task Send(EmailMessageModel emailMessage, Boolean isHtml = true);
    }

    public class EmailMessageModel
    {
        public string ToAddress { get; set; }

        public string Subject { get; set; }

        public string? Body { get; set; }

        public string? AttachmentPath { get; set; }

        public EmailMessageModel(string toAddress, string subject, string? body="") { 
            ToAddress = toAddress;
            Subject = subject;
            Body = body;    
        }
    }

}
