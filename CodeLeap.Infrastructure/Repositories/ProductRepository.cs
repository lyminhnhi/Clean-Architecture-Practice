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

            IQueryable<Product> query = _context.Products;

            if (!string.IsNullOrWhiteSpace(search))
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
            EnsureNotNull(product);

            _logger.LogInformation("Adding new product: {Name}", product.Name);

            await _context.Products.AddAsync(product);
            await SaveChangesAsync("Product added successfully: {Name}", product.Name);
        }

        public async Task UpdateAsync(Product product)
        {
            EnsureNotNull(product);

            _logger.LogInformation("Updating product Id: {Id}", product.Id);

            _context.Products.Update(product);
            await SaveChangesAsync("Product updated successfully: {Id}", product.Id);
        }

        public async Task DeleteAsync(Product product)
        {
            EnsureNotNull(product);

            _logger.LogInformation("Deleting product Id: {Id}", product.Id);

            _context.Products.Remove(product);
            await SaveChangesAsync("Product deleted successfully: {Id}", product.Id);
        }

        private async Task SaveChangesAsync(string successMessage, params object[] args)
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation(successMessage, args);
        }

        private static void EnsureNotNull(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
        }
    }
}