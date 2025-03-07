using NotificationsAndAlerts.Application.Messages;

namespace NotificationsAndAlerts.Application.Interfaces
{
    public interface IEmailNotificationService
    {
        Task SendEmailAsync(EmailNotificationRequest request);
    }
}
