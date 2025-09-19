using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Net;
using System.Net.Mail;
using KBN.Models.EmailModels;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using KBN.Interfaces;

namespace KBN.Services
{
    public class EmailSender : IAttachmentEmailSender
    {

        public readonly SmtpSettings _smtpSettings;

        public EmailSender(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        } 

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                EnableSsl = true,
            };

            var mail = new MailMessage(_smtpSettings.UserName, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };


            return smtp.SendMailAsync(mail);
        }

        public Task SendEmailWithAttachmentAsync(string email, string subject, string htmlMessage, Attachment? attached = null)
        {
            var smtp = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.UserName, _smtpSettings.Password),
                EnableSsl = true,
            };

            var mail = new MailMessage(_smtpSettings.UserName, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            if (attached != null)
            {

                mail.Attachments.Add(attached);
            }
            return smtp.SendMailAsync(mail);
        }
    }
}
