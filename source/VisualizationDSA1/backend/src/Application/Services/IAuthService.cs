using System.Threading.Tasks;
using VisualizationDSA.Application.DTOs;

namespace VisualizationDSA.Application.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> GoogleLoginAsync(string googleEmail, string googleName, string googleSubject);
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);
        Task              LogoutAsync(string refreshToken);
        Task<UserDto>      GetCurrentUserAsync(string userId);
    }
}
