using NotificationsAndAlerts.Application.Messages.common;

namespace NotificationsAndAlerts.Application.Messages
{
    public class EmailNotificationRequest: EmailNotification
    {
        /// <summary>
        /// With id can get email
        /// </summary>
        public int IdUser { get; set; }
    }
}
