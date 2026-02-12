using CodeLeap.Application.DTOs.User;
using FluentAssertions;
using System.Net;
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

        private RegisterRequest CreateValidRegisterRequest(string email)
        {
            return new RegisterRequest
            {
                Email = email,
                Password = "StrongPass123"
            };
        }

        [Fact]
        public async Task Register_ShouldReturnToken_And_RefreshToken()
        {
            var request = CreateValidRegisterRequest("integration1@gmail.com");

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            result.Should().NotBeNull();
            result.Email.Should().Be(request.Email);
            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Register_ShouldFail_WhenEmailAlreadyExists()
        {
            var request = CreateValidRegisterRequest("duplicate@gmail.com");

            await _client.PostAsJsonAsync("/api/auth/register", request);

            var response = await _client.PostAsJsonAsync("/api/auth/register", request);

            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Fact]
        public async Task Login_ShouldReturnToken_AfterRegister()
        {
            var register = CreateValidRegisterRequest("login@gmail.com");

            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginRequest
            {
                Email = register.Email,
                Password = register.Password
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", login);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

            result.Token.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Login_ShouldFail_WithWrongPassword()
        {
            var request = new LoginRequest
            {
                Email = "notexist@gmail.com",
                Password = "WrongPass123"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            response.IsSuccessStatusCode.Should().BeFalse();
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Refresh_ShouldReturnNewAccessToken_AndNewRefreshToken()
        {
            var register = CreateValidRegisterRequest("refresh@gmail.com");

            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginRequest
            {
                Email = register.Email,
                Password = register.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            var refreshRequest = new RefreshRequest
            {
                RefreshToken = auth.RefreshToken
            };

            var refreshResponse = await _client.PostAsJsonAsync(
                "/api/auth/refresh", refreshRequest);

            refreshResponse.EnsureSuccessStatusCode();

            var refreshed = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();

            // ASSERT

            refreshed.Should().NotBeNull();

            refreshed.Token.Should().NotBeNullOrEmpty();

            refreshed.RefreshToken.Should().NotBe(auth.RefreshToken);

            refreshed.Email.Should().Be(auth.Email);
        }

        [Fact]
        public async Task OldRefreshToken_ShouldBeInvalid_AfterRotation()
        {
            var register = CreateValidRegisterRequest("rotate@gmail.com");

            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginRequest
            {
                Email = register.Email,
                Password = register.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            // Refresh token
            var firstRefresh = await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new RefreshRequest { RefreshToken = auth.RefreshToken });

            firstRefresh.EnsureSuccessStatusCode();

            var secondRefresh = await _client.PostAsJsonAsync(
                "/api/auth/refresh",
                new RefreshRequest { RefreshToken = auth.RefreshToken });

            secondRefresh.IsSuccessStatusCode.Should().BeFalse();
        }


        [Fact]
        public async Task Logout_ShouldInvalidateRefreshToken()
        {
            var register = CreateValidRegisterRequest("logout@gmail.com");

            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginRequest
            {
                Email = register.Email,
                Password = register.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", login);

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

            var logoutRequest = new RefreshRequest
            {
                RefreshToken = auth.RefreshToken
            };

            var logoutResponse = await _client.PostAsJsonAsync(
                "/api/auth/logout", logoutRequest);

            logoutResponse.EnsureSuccessStatusCode();

            var refreshResponse = await _client.PostAsJsonAsync(
                "/api/auth/refresh", logoutRequest);

            refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
