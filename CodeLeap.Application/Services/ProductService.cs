using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Interfaces;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository repository,
            ILogger<ProductService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<ProductDto>> GetAll(string? search)
        {
            _logger.LogInformation("GetAll products called with search: {Search}", search);

            var products = await _repository.GetAllAsync(search);

            _logger.LogInformation("GetAll products returned {Count} items", products.Count);

            return products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl
            }).ToList();
        }

        public async Task<ProductDto?> GetById(int id)
        {
            _logger.LogInformation("GetById called for ProductId: {Id}", id);

            var p = await _repository.GetByIdAsync(id);

            if (p == null)
            {
                _logger.LogWarning("Product not found with Id: {Id}", id);
                return null;
            }

            _logger.LogInformation("Product retrieved successfully: {Id}", id);

            return new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ImageUrl = p.ImageUrl
            };
        }

        public async Task Create(CreateProductRequest request)
        {
            _logger.LogInformation("Create product process started. Name: {Name}", request.Name);

            var product = new Product(
                request.Name,
                request.Description,
                request.ImageUrl
            );

            if (!product.IsValid())
            {
                _logger.LogWarning("Product creation failed - invalid data. Name: {Name}", request.Name);
                throw new ArgumentException("Invalid product data");
            }

            await _repository.AddAsync(product);

            _logger.LogInformation("Product created successfully with Name: {Name}", request.Name);
        }

        public async Task Update(int id, UpdateProductRequest request)
        {
            _logger.LogInformation("Update product process started for Id: {Id}", id);

            var product = await _repository.GetByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Update failed - product not found with Id: {Id}", id);
                throw new KeyNotFoundException("Product not found");
            }

            product.Name = request.Name;
            product.Description = request.Description;
            product.ImageUrl = request.ImageUrl;

            await _repository.UpdateAsync(product);

            _logger.LogInformation("Product updated successfully with Id: {Id}", id);
        }

        public async Task Delete(int id)
        {
            _logger.LogInformation("Delete product process started for Id: {Id}", id);

            var product = await _repository.GetByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Delete failed - product not found with Id: {Id}", id);
                throw new KeyNotFoundException("Product not found");
            }

            await _repository.DeleteAsync(product);

            _logger.LogInformation("Product deleted successfully with Id: {Id}", id);
        }
    }
}