using CodeLeap.Application.DTOs.User;
using CodeLeap.Application.Interfaces;
using CodeLeap.Application.Services;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace CodeLeap.UnitTests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IJwtHelper> _jwtMock;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _jwtMock = new Mock<IJwtHelper>();

            _service = new UserService(_userRepoMock.Object, _jwtMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnToken_WhenUserIsValid()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@gmail.com",
                Password = "123456"
            };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            _jwtMock
                .Setup(x => x.GenerateToken(request.Email))
                .Returns("fake-token");

            // Act
            var result = await _service.Register(request);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            result.Token.Should().Be("fake-token");

            _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task Register_ShouldThrowException_WhenEmailAlreadyExists()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "test@gmail.com",
                Password = "123456"
            };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(new User("test@gmail.com", "123456"));

            // Act
            Func<Task> act = async () => await _service.Register(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Email already exists");
        }

        [Fact]
        public async Task Login_ShouldReturnToken_WhenCredentialsAreCorrect()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "test@gmail.com",
                Password = "123456"
            };

            var user = new User("test@gmail.com", "123456");

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync(user);

            _jwtMock
                .Setup(x => x.GenerateToken(request.Email))
                .Returns("jwt-token");

            // Act
            var result = await _service.Login(request);

            // Assert
            result.Email.Should().Be(request.Email);
            result.Token.Should().Be("jwt-token");
        }

        [Fact]
        public async Task Login_ShouldThrowException_WhenCredentialsInvalid()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "wrong@gmail.com",
                Password = "123456"
            };

            _userRepoMock
                .Setup(x => x.GetByEmailAsync(request.Email))
                .ReturnsAsync((User)null);

            // Act
            Func<Task> act = async () => await _service.Login(request);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Invalid credentials");
        }
    }
}
