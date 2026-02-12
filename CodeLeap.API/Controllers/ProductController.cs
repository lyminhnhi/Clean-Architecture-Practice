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

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Creating new product with name: {ProductName}", request.Name);

            await _service.Create(request);

            return Created(string.Empty, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Updating product with id: {ProductId}", id);

            await _service.Update(id, request);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting product with id: {ProductId}", id);

            await _service.Delete(id);

            return NoContent();
        }
    }
}