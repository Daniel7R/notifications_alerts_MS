namespace NotificationsAndAlerts.Application.Queues
{
    public static class Queues
    {
        public const string SEND_EMAIL_NOTIFICATION_SALE = "ticket.sale.notification";
        public const string GET_USER_BY_ID = "user.by_id";

        //tournament
        public const string SEND_EMAIL_CREATE_TOURNAMENT = "tournament.created";
        public const string SEND_EMAIL_UPDATE_TOURNAMENT = "tournament.update";
        public const string REMINDER = "matches.tournament.reminder";
        public const string SEND_EMAIL_MATCH_WINNER= "match.winner";


        //Payments
        public const string SEND_EMAIL_NOTIFICATION_PAYMENT = "payment.notification";
        public const string SEND_EMAIL_DONATION = "donation.email";

        //users
        public const string ALL_USER_EMAILS = "users.emails";
    }
}
