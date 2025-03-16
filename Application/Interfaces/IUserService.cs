using NotificationsAndAlerts.Application.Messages;

namespace NotificationsAndAlerts.Application.Interfaces
{
    public interface IUserService
    {
        Task<GetUserByIdResponse> GetUserInfo(int idUser);
        Task<List<string>> GetAllUserEmails();
    }
}
