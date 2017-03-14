using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace LoanToolIoT.Controllers
{
    public sealed class EmailController
    {
        public IAsyncOperation<bool> SendEmail(string Email, string subject, string message)
        {
            return PSendEmail(Email, subject, message).AsAsyncOperation();
        }
        private async Task<bool> PSendEmail(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("AXIS Virtual Loan tool", "usadploan@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("plain") { Text = message };

            using (var client = new SmtpClient())
            {
                client.LocalDomain = "smpt.gmail.com";
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync("usadploan", "?=9&7zVUN_F.K,AX3rfUw7fNMF");
                await client.ConnectAsync("smtp.gmail.com", 465, SecureSocketOptions.Auto).ConfigureAwait(false);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
            return true;
        }
    }
}
