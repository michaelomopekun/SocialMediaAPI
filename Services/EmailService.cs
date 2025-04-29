
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailService : IEmailService
{

    public async Task SendEmailAsync(string email, string subject, string body)
    {
        try
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(Environment.GetEnvironmentVariable("EMAIL_FROM")));
            mimeMessage.To.Add(MailboxAddress.Parse(email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            
            var port = Environment.GetEnvironmentVariable("EMAIL_PORT");
            if (string.IsNullOrEmpty(port))
            {
                throw new Exception("Port is not configured in EmailSettings");
            }

            await smtp.ConnectAsync(Environment.GetEnvironmentVariable("EMAIL_HOST"), int.Parse(port), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(Environment.GetEnvironmentVariable("EMAIL_USERNAME"), Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));
            await smtp.SendAsync(mimeMessage);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            throw new Exception("Error sending email", ex);
        }
    }
}