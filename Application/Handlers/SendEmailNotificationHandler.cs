using NotificationsAndAlerts.Application.Interfaces;
using NotificationsAndAlerts.Application.Messages;
using NotificationsAndAlerts.Application.Messages.common;

namespace NotificationsAndAlerts.Application.Handlers
{
    public class SendEmailNotificationHandler
    {
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IEventBusProducer _eventBusProducer;
        private readonly ILogger<SendEmailNotificationHandler> _logger;

        public SendEmailNotificationHandler(IEmailNotificationService emailNotificationService, IEventBusProducer eventBusProducer, ILogger<SendEmailNotificationHandler> logger)
        {
            _emailNotificationService = emailNotificationService;
            _eventBusProducer = eventBusProducer;
            _logger = logger;
        }

        public async Task HandleAsync(EmailNotificationRequest request)
        {
            try
            {
                await _emailNotificationService.SendEmailAsync(request);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing email notification: {ex.Message}");
                throw;
            }
        }

        public async Task HandleCreationTournament(EmailNotification payload)
        {
            try
            {
                List<int> usersIds = await _eventBusProducer.SendRequest<object?, List<int>>(null, Queues.Queues.ALL_USER_EMAILS);
                List<EmailNotificationRequest> requestList = usersIds.Select(x => new EmailNotificationRequest
                {
                    Body = payload.Body,
                    Subject = payload.Subject,
                    IdUser = x
                }).ToList();

                requestList.ForEach(async request =>
                {
                    await _emailNotificationService.SendEmailAsync(request); 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing email notification: {ex.Message}");
                throw;
            }

        }
    }
}
