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

        public WebhookServices(ApplicationDbContext context, IConfiguration configuration, ILogger<WebhookServices> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>();
        }

        private async Task SendMailAsync(string subject, string body)
        {

            try
            {

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

        public bool ValidateSignature(string payload, string signature, string secretKey)
        {
            try
            {
                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogWarning("Missing signature or secret key for webhook validation.");
                    return false;
                }

                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
                var computedSignature = $"sha256={BitConverter.ToString(computedHash).Replace("-", "").ToLower()}";

                bool isValid = computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);
                if (!isValid)
                {
                    _logger.LogWarning("Invalid webhook signature.");
                }
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature.");
                return false;
            }
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
