using CodeLeap.Application.DTOs.User;
using FluentAssertions;
using System.Net.Http.Json;

namespace CodeLeap.IntegrationTests
{
    public class AuthControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_ShouldReturnToken()
        {
            var request = new RegisterRequest
            {
                Email = "integration@gmail.com",
                Password = "123456"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/auth/register", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldReturnToken_AfterRegister()
        {
            var register = new RegisterRequest
            {
                Email = "login@gmail.com",
                Password = "123456"
            };

            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginRequest
            {
                Email = "login@gmail.com",
                Password = "123456"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/auth/login", login);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldFail_WithWrongPassword()
        {
            var request = new LoginRequest
            {
                Email = "notexist@gmail.com",
                Password = "wrong"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/auth/login", request);

            response.IsSuccessStatusCode.Should().BeFalse();
        }
    }
}
