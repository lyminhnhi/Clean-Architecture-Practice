using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Services;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CodeLeap.UnitTests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly Mock<ILogger<ProductService>> _loggerMock;
        private readonly ProductService _service;
        private readonly string _imgTest = "http://test.com";

        public ProductServiceTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _loggerMock = new Mock<ILogger<ProductService>>();

            _service = new ProductService(
                _repoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnListOfProducts()
        {
            var products = new List<Product>
            {
                new Product("A", "Desc A", _imgTest),
                new Product("B", "Desc B", _imgTest)
            };

            _repoMock
                .Setup(x => x.GetAllAsync(null))
                .ReturnsAsync(products);

            var result = await _service.GetAll(null);

            result.Should().HaveCount(2);
            result[0].Name.Should().Be("A");
        }

        [Fact]
        public async Task GetById_ShouldReturnProduct_WhenExists()
        {
            var product = new Product("A", "Desc", _imgTest);

            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(product);

            var result = await _service.GetById(1);

            result.Should().NotBeNull();
            result.Name.Should().Be("A");
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotFound()
        {
            _repoMock
                .Setup(x => x.GetByIdAsync(99))
                .ReturnsAsync((Product)null);

            var result = await _service.GetById(99);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Create_ShouldCallRepository_WhenProductValid()
        {
            var request = new CreateProductRequest
            {
                Name = "Test",
                Description = "Desc",
                ImageUrl = _imgTest
            };

            await _service.Create(request);

            _repoMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenProductNotFound()
        {
            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync((Product)null);

            var request = new UpdateProductRequest
            {
                Name = "A",
                Description = "B",
                ImageUrl = _imgTest
            };

            Func<Task> act = async () => await _service.Update(1, request);

            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Product not found");
        }

        [Fact]
        public async Task Delete_ShouldCallRepository_WhenProductExists()
        {
            var product = new Product("A", "B", _imgTest);

            _repoMock
                .Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(product);

            await _service.Delete(1);

            _repoMock.Verify(x => x.DeleteAsync(product), Times.Once);
        }
    }
}
