using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IProductService service,
            ILogger<ProductController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(string? search)
        {
            _logger.LogInformation("Fetching all products. Search term: {Search}", search);

            var result = await _service.GetAll(search);

            _logger.LogInformation("Returned {Count} products", result?.Count());

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            _logger.LogInformation("Fetching product with id: {ProductId}", id);

            var product = await _service.GetById(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found with id: {ProductId}", id);
                return NotFound();
            }

            _logger.LogInformation("Product found with id: {ProductId}", id);

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductRequest request)
        {
            _logger.LogInformation("Creating new product with name: {ProductName}", request.Name);

            await _service.Create(request);

            _logger.LogInformation("Product created successfully: {ProductName}", request.Name);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateProductRequest request)
        {
            _logger.LogInformation("Updating product with id: {ProductId}", id);

            await _service.Update(id, request);

            _logger.LogInformation("Product updated successfully: {ProductId}", id);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting product with id: {ProductId}", id);

            await _service.Delete(id);

            _logger.LogInformation("Product deleted successfully: {ProductId}", id);

            return Ok();
        }
    }
}