using NotificationsAndAlerts.Application.Interfaces;
using NotificationsAndAlerts.Application.Messages;

namespace NotificationsAndAlerts.Application.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IEventBusProducer _eventBusProducer;
        public UserService(IEventBusProducer eventBusProducer, ILogger<UserService> logger) 
        {
            _logger = logger;
            _eventBusProducer = eventBusProducer;
        }
        public async Task<GetUserByIdResponse> GetUserInfo(int idUser)
        {
            try
            {
                GetUserByIdResponse response = await _eventBusProducer.SendRequest<int, GetUserByIdResponse>(idUser, Queues.Queues.GET_USER_BY_ID);

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<string>> GetAllUserEmails()
        {
            try
            {
                List<string> emails = await _eventBusProducer.SendRequest<object?, List<string>>(null, Queues.Queues.ALL_USER_EMAILS);

                return emails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw new Exception(ex.Message);
            }
        }
    }
}
