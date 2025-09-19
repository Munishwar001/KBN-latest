using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace KBN.Interfaces
{
    public interface IAttachmentEmailSender:IEmailSender
    {
        Task SendEmailWithAttachmentAsync(
           string email,
           string subject,
           string htmlMessage,
           Attachment attached);
    }
}
