namespace MyApp.Services
{
  // Services/MailService.cs
  using MailKit.Net.Smtp;
  using MimeKit;
  using Microsoft.Extensions.Options;

  public class EmailService
  {
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpOptions)
    {
      _smtpSettings = smtpOptions.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
      var email = new MimeMessage();
      email.From.Add(MailboxAddress.Parse(_smtpSettings.Username));
      email.To.Add(MailboxAddress.Parse(to));
      email.Subject = subject;

      email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

      using var smtp = new SmtpClient();
      await smtp.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port,
          _smtpSettings.UseSSL ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.None);
      await smtp.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
      await smtp.SendAsync(email);
      await smtp.DisconnectAsync(true);
    }
  }
}
