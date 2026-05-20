namespace MyApp.Services
{
  using MailKit.Net.Smtp;
  using MimeKit;
  using Microsoft.Extensions.Options;
  using MyApp.Constants;
  using MyApp.Helper.DB;

  public class EmailService
  {
    private readonly SmtpSettings _smtpSettings;
    private readonly EmailTemplateDbHelper _emailTemplateDb;

    public EmailService(IOptions<SmtpSettings> smtpOptions, EmailTemplateDbHelper emailTemplateDb)
    {
      _smtpSettings = smtpOptions.Value;
      _emailTemplateDb = emailTemplateDb;
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

    public async Task<bool> SendAdminForgotPasswordAsync(string to, string fullName, string resetLink, string langCode)
    {
      var template = await _emailTemplateDb.GetByKeyAsync(EmailTemplateConstants.AdminForgotPassword, langCode);
      if (template == null) return false;

      var subject = template.Subject.Replace("{{FullName}}", fullName);
      var body = template.BodyHtml
          .Replace("{{FullName}}", fullName)
          .Replace("{{ResetLink}}", resetLink);

      await SendEmailAsync(to, subject, body);
      return true;
    }

    public async Task<bool> SendCustomerForgotPasswordAsync(string to, string fullName, string resetLink, string langCode)
    {
      var template = await _emailTemplateDb.GetByKeyAsync(EmailTemplateConstants.CustomerForgotPassword, langCode);
      if (template == null) return false;

      var subject = template.Subject.Replace("{{FullName}}", fullName);
      var body = template.BodyHtml
          .Replace("{{FullName}}", fullName)
          .Replace("{{ResetLink}}", resetLink);

      await SendEmailAsync(to, subject, body);
      return true;
    }
  }
}
