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
                throw new ArgumentException("Invalid email");
            }

            await _userRepository.AddAsync(user);

            _logger.LogInformation("User registered successfully: {Email}", request.Email);

            var accessToken = _jwtHelper.GenerateToken(user.Email);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                Email = user.Email,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            return new AuthResponse
            {
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshToken
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

            if (!SecurityHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed - Invalid password for email: {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);

            var accessToken = _jwtHelper.GenerateToken(user.Email);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                Email = user.Email,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            return new AuthResponse
            {
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse> RefreshToken(string refreshToken)
        {
            var stored = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (stored == null ||
                stored.ExpiryDate < DateTime.UtcNow ||
                stored.IsRevoked)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Revoke old token
            await _refreshTokenRepository.RevokeAsync(refreshToken);

            // Generate new pair
            var newAccessToken = _jwtHelper.GenerateToken(stored.Email);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken();

            await _refreshTokenRepository.AddAsync(new RefreshToken
            {
                Token = newRefreshToken,
                Email = stored.Email,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            });

            return new AuthResponse
            {
                Email = stored.Email,
                Token = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        public async Task Logout(string refreshToken)
        {
            await _refreshTokenRepository.RevokeAsync(refreshToken);
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
