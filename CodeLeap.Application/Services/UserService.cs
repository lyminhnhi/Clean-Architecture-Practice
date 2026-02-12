using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtHelper _jwtHelper;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IJwtHelper jwtHelper,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        public async Task<AuthResponse> Register(RegisterRequest request)
        {
            _logger.LogInformation("Register process started for email: {Email}", request.Email);

            var existing = await _userRepository.GetByEmailAsync(request.Email);

            if (existing != null)
            {
                _logger.LogWarning("Registration failed - Email already exists: {Email}", request.Email);
                throw new InvalidOperationException("Email already exists");
            }

            var user = new User(request.Email, request.Password);

            if (!user.IsValidEmail())
            {
                _logger.LogWarning("Registration failed - Invalid email format: {Email}", request.Email);
                throw new ArgumentException("Invalid email");
            }

            await _userRepository.AddAsync(user);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return new AuthResponse
            {
                Email = user.Email,
                Token = _jwtHelper.GenerateToken(user.Email)
            };
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed - User not found: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (user.PasswordHash != request.Password)
            {
                _logger.LogWarning("Login failed - Invalid password for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return new AuthResponse
            {
                Email = user.Email,
                Token = _jwtHelper.GenerateToken(user.Email)
            };
        }
    }
}
