using NotificationsAndAlerts.Application.Interfaces;
using NotificationsAndAlerts.Application.Messages;
using System.Net.Mail;
using System.Net;
using NotificationsAndAlerts.Application.Configs;
using Microsoft.Extensions.Options;

namespace NotificationsAndAlerts.Application.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly SmtpConfig _smtpConfig;
        private readonly IUserService _userService;
        public EmailNotificationService(IOptions<SmtpConfig> options, IUserService userService ,ILogger<EmailNotificationService> logger)
        {
            _userService = userService;
            _smtpConfig = options.Value;
            _logger = logger;
        }
        public async Task SendEmailAsync(EmailNotificationRequest request)
        {
            try
            {
                var userInfo = await _userService.GetUserInfo(request.IdUser);
                var smtpClient = new SmtpClient(_smtpConfig.SMTP_CLIENT)
                {
                    Port = _smtpConfig.PORT,
                    Credentials = new NetworkCredential(_smtpConfig.USER_EMAIL, _smtpConfig.PASSWORD_APP),
                    EnableSsl = true
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpConfig.USER_EMAIL),
                    Subject = request.Subject,
                    Body = request.Body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(userInfo.Email);
                smtpClient.Send(mailMessage); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"error with {request.IdUser} : {ex.Message}");
                throw new Exception(ex.Message);
            }

        }
    }
}
