using MimeKit;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using WebhookReceiver.Data;
using WebhookReceiver.Models;

namespace WebhookReceiver.Services
{
    public class WebhookServices
    {

        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;

        private readonly ILogger<WebhookServices> _logger;

        private readonly EmailSettings _emailSettings;

        private readonly string _githubSecret;
        public WebhookServices(ApplicationDbContext context, IConfiguration configuration, ILogger<WebhookServices> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _githubSecret = configuration["GitHubSecret"] ?? throw new ArgumentNullException(nameof(_githubSecret));
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        }

        private async Task SendMailAsync(string subject, string body)
        {

            try
            {
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress("Alerts", _emailSettings.SenderEmail));
                email.To.Add(new MailboxAddress("", _emailSettings.RecipientEmail));
                email.Subject = subject;
                email.Body = new TextPart("plain") { Text = body };

                var smtp = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort);
                smtp.EnableSsl = true;
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.SenderPassword);

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailSettings.SenderEmail);
                mailMessage.To.Add(_emailSettings.RecipientEmail);
                mailMessage.Subject = subject;
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = body;

                await smtp.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unable to send the mail", ex.Message);
                throw ex;
            }
            
            
        }

        public bool ValidateGithubSignature(string payload, string signature)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_githubSecret));
            var computedHash=hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature=$"sha256={BitConverter.ToString(computedHash).Replace("-","").ToLower()}";
            return computedSignature.Equals(computedHash);
        }
        public async Task<Boolean> SaveAlert(Alert alert)
        {
            try
            {
                _context.Alerts.Add(alert);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Alert saved to database");

                string emailBody = $@"Azure Alert Notifications\n
                                        Alert Name: {alert.AlertName}\n
                                        Severity: {alert.Severity}\n
                                        Resource: {alert.Resource}\n
                                        Description: {alert.Description}\n
                                        TimeStamp: {alert.TimeStamp}
                                    ";
                await SendMailAsync($"Alert {alert.AlertName}", emailBody);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return false;
            }
        }
        
    }
}
