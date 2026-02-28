using CodeLeap.Application.DTOs.Product;

namespace CodeLeap.Application.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductDto>> GetAll(string? search);
        Task<ProductDto?> GetById(int id);
        Task<int> Create(CreateProductRequest request);
        Task Update(int id, UpdateProductRequest request);
        Task Delete(int id);
    }
}
