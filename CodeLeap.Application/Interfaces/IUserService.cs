using CodeLeap.Application.DTOs.User;

namespace CodeLeap.Application.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponse> Register(RegisterRequest request);
        Task<AuthResponse> Login(LoginRequest request);
        Task<AuthResponse> RefreshToken(string refreshToken);
        Task Logout(string refreshToken);
    }
}
