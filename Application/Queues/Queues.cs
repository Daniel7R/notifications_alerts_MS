﻿namespace NotificationsAndAlerts.Application.Queues
{
    public static class Queues
    {
        public const string SEND_EMAIL_NOTIFICATION_SALE = "ticket.sale.notification";
        public const string GET_USER_BY_ID = "user.by_id";

        //tournament
        public const string SEND_EMAIL_CREATE_TOURNAMENT = "tournament.created";

        //Payments
        public const string SEND_EMAIL_NOTIFICATION_PAYMENT = "payment.notification";
        public const string SEND_EMAIL_DONATION = "donation.email";

        //users
        public const string ALL_USER_EMAILS = "users.emails";
    }
}
