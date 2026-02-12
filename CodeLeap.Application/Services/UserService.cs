using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using CodeLeap.Application.Security;

namespace CodeLeap.Application.Services
{
    public class UserService : IUserService
    {
        private const int RefreshTokenExpiryDays = 7;

        private readonly IUserRepository _userRepository;
        private readonly IJwtHelper _jwtHelper;
        private readonly ILogger<UserService> _logger;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public UserService(
            IUserRepository userRepository,
            IJwtHelper jwtHelper,
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _refreshTokenRepository = refreshTokenRepository;
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

            ValidatePassword(request.Password);

            var hashedPassword = SecurityHelper.HashPassword(request.Password);
            var user = new User(request.Email, hashedPassword);

            if (!user.IsValidEmail())
            {
                _logger.LogWarning("Registration failed - Invalid email format: {Email}", request.Email);
                throw new ArgumentException("Invalid email format");
            }

            await _userRepository.AddAsync(user);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null ||
                !SecurityHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed - Invalid credentials for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            return await GenerateAuthResponseAsync(user);
        }

        public async Task<AuthResponse> RefreshToken(string refreshToken)
        {
            _logger.LogInformation("Refresh token attempt");

            var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (stored == null ||
                stored.ExpiryDate < DateTime.UtcNow ||
                stored.IsRevoked)
            {
                _logger.LogWarning("Invalid or expired refresh token");
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            await _refreshTokenRepository.RevokeAsync(refreshToken);

            _logger.LogInformation("Refresh token rotated for email: {Email}", stored.Email);

            return await GenerateAuthResponseAsync(new User(stored.Email, string.Empty));
        }

        public async Task Logout(string refreshToken)
        {
            _logger.LogInformation("Logout attempt");

            await _refreshTokenRepository.RevokeAsync(refreshToken);

            _logger.LogInformation("Refresh token revoked successfully");
        }

        private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
        {
            var accessToken = _jwtHelper.GenerateToken(user.Email);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                Email = user.Email,
                ExpiryDate = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays),
                IsRevoked = false
            });

            return new AuthResponse
            {
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }

        private void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                throw new ArgumentException("Password must be at least 8 characters long");

            if (!password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit))
                throw new ArgumentException("Password must contain uppercase, lowercase and number");
        }
    }
}