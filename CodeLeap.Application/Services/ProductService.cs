using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Interfaces;
using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;

namespace CodeLeap.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ProductDto>> GetAll(string? search)
        {
            var products = await _repository.GetAllAsync(search);

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
            var p = await _repository.GetByIdAsync(id);

            if (p == null) return null;

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
            var product = new Product(
                request.Name,
                request.Description,
                request.ImageUrl
            );

            if (!product.IsValid())
                throw new Exception("Invalid product");

            await _repository.AddAsync(product);
        }

        public async Task Update(int id, UpdateProductRequest request)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            product.Name = request.Name;
            product.Description = request.Description;
            product.ImageUrl = request.ImageUrl;

            await _repository.UpdateAsync(product);
        }

        public async Task Delete(int id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                throw new Exception("Product not found");

            await _repository.DeleteAsync(product);
        }
    }
}
