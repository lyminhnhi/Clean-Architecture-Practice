using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;

namespace CodeLeap.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtHelper _jwtHelper;

        public UserService(IUserRepository userRepository, IJwtHelper jwtHelper)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email);

            if (existing != null)
                throw new Exception("Email already exists");

            var user = new User(request.Email, request.Password);

            if (!user.IsValidEmail())
                throw new Exception("Invalid email");

            await _userRepository.AddAsync(user);

            return new AuthResponse
            {
                Email = user.Email,
                Token = _jwtHelper.GenerateToken(user.Email)
            };
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || user.PasswordHash != request.Password)
                throw new Exception("Invalid credentials");

            return new AuthResponse
            {
                Email = user.Email,
                Token = _jwtHelper.GenerateToken(user.Email)
            };
        }
    }
}
