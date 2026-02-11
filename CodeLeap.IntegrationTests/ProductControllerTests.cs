using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.DTOs.User;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace CodeLeap.IntegrationTests
{
    public class ProductControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ProductControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsync()
        {
            var register = new RegisterRequest
            {
                Email = "product@gmail.com",
                Password = "123456"
            };

            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginRequest
            {
                Email = "product@gmail.com",
                Password = "123456"
            };

            var response = await _client.PostAsJsonAsync(
                "/api/auth/login", login);

            var result = await response.Content
                .ReadFromJsonAsync<AuthResponse>();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", result.Token);
        }

        [Fact]
        public async Task GetAll_ShouldReturnUnauthorized_WithoutToken()
        {
            var response = await _client.GetAsync("/api/product");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_And_Get_Product_Flow()
        {
            await AuthenticateAsync();

            var createRequest = new CreateProductRequest
            {
                Name = "Integration Product",
                Description = "Test Desc",
                ImageUrl = "http://test.com"
            };

            var createResponse = await _client.PostAsJsonAsync(
                "/api/product", createRequest);

            createResponse.EnsureSuccessStatusCode();

            var getResponse = await _client.GetAsync("/api/product");

            getResponse.EnsureSuccessStatusCode();

            var products = await getResponse.Content
                .ReadFromJsonAsync<List<ProductDto>>();

            products.Should().NotBeEmpty();
            products.First().Name.Should().Be("Integration Product");
        }

        [Fact]
        public async Task Update_Product_ShouldSucceed()
        {
            await AuthenticateAsync();

            var createRequest = new CreateProductRequest
            {
                Name = "Old Name",
                Description = "Old",
                ImageUrl = "old"
            };

            await _client.PostAsJsonAsync("/api/product", createRequest);

            var products = await _client.GetFromJsonAsync<List<ProductDto>>(
                "/api/product");

            var id = products.First().Id;

            var updateRequest = new UpdateProductRequest
            {
                Name = "New Name",
                Description = "New",
                ImageUrl = "new"
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/product/{id}", updateRequest);

            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Delete_Product_ShouldSucceed()
        {
            await AuthenticateAsync();

            var createRequest = new CreateProductRequest
            {
                Name = "To Delete",
                Description = "Del",
                ImageUrl = "del"
            };

            await _client.PostAsJsonAsync("/api/product", createRequest);

            var products = await _client.GetFromJsonAsync<List<ProductDto>>(
                "/api/product");

            var id = products.First().Id;

            var response = await _client.DeleteAsync($"/api/product/{id}");

            response.EnsureSuccessStatusCode();
        }
    }
}
