using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.Services;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using CodeLeap.Application.Security;

namespace CodeLeap.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IJwtHelper> _jwtMock;
        private readonly Mock<IRefreshTokenRepository> _refreshRepoMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtMock = new Mock<IJwtHelper>();
            _refreshRepoMock = new Mock<IRefreshTokenRepository>();
            _loggerMock = new Mock<ILogger<UserService>>();

            _service = new UserService(
                _userRepoMock.Object,
                _jwtMock.Object,
                _refreshRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnTokenAndRefreshToken_WhenUserIsValid()
        {
            var request = new RegisterRequest
            {
                Email = "test@gmail.com",
                Password = "StrongPass123"
            };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            _jwtMock
                .Setup(x => x.GenerateToken(request.Email))
                .Returns("fake-access-token");

            _jwtMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("fake-refresh-token");

            var result = await _service.Register(request);

            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            result.Token.Should().Be("fake-access-token");
            result.RefreshToken.Should().Be("fake-refresh-token");

            _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
            _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldThrowException_WhenEmailAlreadyExists()
        {
            var request = new RegisterRequest
            {
                Email = "test@gmail.com",
                Password = "StrongPass123"
            };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(new User("test@gmail.com", "hashed"));

            Func<Task> act = async () => await _service.Register(request);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Email already exists");
        }

        [Fact]
        public async Task Login_ShouldReturnTokens_WhenCredentialsAreCorrect()
        {
            var request = new LoginRequest
            {
                Email = "test@gmail.com",
                Password = "StrongPass123"
            };

            var hashed = SecurityHelper.HashPassword(request.Password);

            var user = new User("test@gmail.com", hashed);

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _jwtMock
                .Setup(x => x.GenerateToken(request.Email))
                .Returns("access-token");

            _jwtMock
                .Setup(x => x.GenerateRefreshToken())
                .Returns("refresh-token");

            var result = await _service.Login(request);

            result.Email.Should().Be(request.Email);
            result.Token.Should().Be("access-token");
            result.RefreshToken.Should().Be("refresh-token");

            _refreshRepoMock.Verify(x => x.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldThrowException_WhenUserNotFound()
        {
            var request = new LoginRequest
            {
                Email = "wrong@gmail.com",
                Password = "StrongPass123"
            };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            Func<Task> act = async () => await _service.Login(request);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid credentials");
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnNewAccessToken_WhenValid()
        {
            var token = "valid-refresh";

            _refreshRepoMock
                .Setup(x => x.GetByTokenAsync(token))
                .ReturnsAsync(new RefreshToken
                {
                    Token = token,
                    Email = "test@gmail.com",
                    ExpiryDate = DateTime.UtcNow.AddDays(1),
                    IsRevoked = false
                });

            _jwtMock
                .Setup(x => x.GenerateToken("test@gmail.com"))
                .Returns("new-access-token");

            var result = await _service.RefreshToken(token);

            result.Token.Should().Be("new-access-token");
            result.Email.Should().Be("test@gmail.com");
        }

        [Fact]
        public async Task Logout_ShouldCallRepository()
        {
            await _service.Logout("some-token");

            _refreshRepoMock.Verify(x => x.RevokeAsync("some-token"), Times.Once);
        }
    }
}
