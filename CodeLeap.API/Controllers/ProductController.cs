using CodeLeap.Application.DTOs.Product;
using CodeLeap.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeLeap.API.Controllers
{
    [ApiController]
    [Route("api/products")]
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

        // GET /api/products?search=
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            _logger.LogInformation("Fetching products. Search: {Search}", search);

            var result = await _service.GetAll(search);
            return Ok(result);
        }

        // GET /api/products/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid product id");

            var product = await _service.GetById(id);

            if (product == null)
            {
                _logger.LogWarning("Product not found. Id: {Id}", id);
                return NotFound();
            }

            return Ok(product);
        }

        // POST /api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _logger.LogInformation("Creating product: {Name}", request.Name);

            var createdId = await _service.Create(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdId },
                null
            );
        }

        // PUT /api/products/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductRequest request)
        {
            if (id <= 0)
                return BadRequest("Invalid product id");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _service.GetById(id);
            if (exists == null)
            {
                _logger.LogWarning("Update failed. Product not found: {Id}", id);
                return NotFound();
            }

            await _service.Update(id, request);
            return NoContent();
        }

        // DELETE /api/products/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid product id");

            var exists = await _service.GetById(id);
            if (exists == null)
            {
                _logger.LogWarning("Delete failed. Product not found: {Id}", id);
                return NotFound();
            }

            await _service.Delete(id);
            return NoContent();
        }
    }
}