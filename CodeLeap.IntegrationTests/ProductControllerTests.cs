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

        private async Task AuthenticateAsync(string email)
        {
            var register = new RegisterRequest
            {
                Email = email,
                Password = "StrongPass123"
            };

            await _client.PostAsJsonAsync("/api/auth/register", register);

            var login = new LoginRequest
            {
                Email = email,
                Password = register.Password
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", login);

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

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
            await AuthenticateAsync("product1@gmail.com");

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
            products.Should().Contain(p => p.Name == "Integration Product");
        }

        [Fact]
        public async Task Update_Product_ShouldSucceed()
        {
            await AuthenticateAsync("product2@gmail.com");

            var createRequest = new CreateProductRequest
            {
                Name = "Old Name",
                Description = "Old",
                ImageUrl = "http://example.com/old.jpg"
            };

            var createResponse = await _client.PostAsJsonAsync(
                "/api/product", createRequest);

            if (!createResponse.IsSuccessStatusCode)
            {
                var error = await createResponse.Content.ReadAsStringAsync();
                throw new Exception("Create failed: " + error);
            }

            createResponse.EnsureSuccessStatusCode();

            var products = await _client.GetFromJsonAsync<List<ProductDto>>(
                "/api/product");

            products.Should().NotBeNull();
            products.Should().NotBeEmpty("product should be created before update");

            var id = products.First().Id;

            var updateRequest = new UpdateProductRequest
            {
                Name = "New Name",
                Description = "New",
                ImageUrl = "http://example.com/new.jpg"
            };

            var response = await _client.PutAsJsonAsync(
                $"/api/product/{id}", updateRequest);

            response.EnsureSuccessStatusCode();

            var updated = await _client.GetFromJsonAsync<ProductDto>(
                $"/api/product/{id}");

            updated.Should().NotBeNull();
            updated.Name.Should().Be("New Name");
        }

        [Fact]
        public async Task Delete_Product_ShouldSucceed()
        {
            await AuthenticateAsync("product3@gmail.com");

            var createRequest = new CreateProductRequest
            {
                Name = "To Delete",
                Description = "Del",
                ImageUrl = "http://example.com/old.jpg"
            };

            await _client.PostAsJsonAsync("/api/product", createRequest);

            var products = await _client.GetFromJsonAsync<List<ProductDto>>(
                "/api/product");

            var id = products.First().Id;

            var response = await _client.DeleteAsync($"/api/product/{id}");

            response.EnsureSuccessStatusCode();

            var afterDelete = await _client.GetFromJsonAsync<List<ProductDto>>(
                "/api/product");

            afterDelete.Should().NotContain(p => p.Id == id);
        }
    }
}
