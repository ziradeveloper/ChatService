using ChatService.Domain.Models;
using ChatService.Models;
using ChatService.Settings.JWT;

namespace ChatService.Application.Interfaces
{
    public interface IUserService
    {
        Task<JwtResponseModel> RegisterAsync(RegisterRequest request);
        Task<JwtResponseModel> GoogleLoginAsync(string email, string username, int profileIndex);
        Task<JwtResponseModel> LoginAsync(string email, string password);
        Task UpdateOpenAiTokenAsync(int userId, string token);
        Task<User> GetByIdAsync(int userId);
    }
}
