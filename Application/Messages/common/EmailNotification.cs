namespace NotificationsAndAlerts.Application.Messages.common
{
    public abstract class EmailNotification
    {
        /// <summary>
        ///  Email subject
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        ///  Body of the email
        /// </summary>
        public string Body { get; set; }
    }
}
