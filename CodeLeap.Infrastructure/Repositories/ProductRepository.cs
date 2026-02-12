using CodeLeap.Domain.Entities;
using CodeLeap.Domain.Interfaces;
using CodeLeap.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CodeLeap.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(AppDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Product>> GetAllAsync(string? search)
        {
            _logger.LogInformation("Getting all products with search: {Search}", search);

            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search));
            }

            var result = await query.ToListAsync();

            _logger.LogInformation("GetAllAsync returned {Count} products", result.Count);

            return result;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            _logger.LogDebug("Querying product by Id: {Id}", id);

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found with Id: {Id}", id);
            }
            else
            {
                _logger.LogInformation("Product found with Id: {Id}", id);
            }

            return product;
        }

        public async Task AddAsync(Product product)
        {
            _logger.LogInformation("Adding new product: {Name}", product.Name);

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product added successfully: {Name}", product.Name);
        }

        public async Task UpdateAsync(Product product)
        {
            _logger.LogInformation("Updating product Id: {Id}", product.Id);

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated successfully: {Id}", product.Id);
        }

        public async Task DeleteAsync(Product product)
        {
            _logger.LogInformation("Deleting product Id: {Id}", product.Id);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product deleted successfully: {Id}", product.Id);
        }
    }
}
