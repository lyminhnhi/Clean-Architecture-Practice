namespace CodeLeap.Application.DTOs.Product
{
    public class UpdateProductRequest
    {
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string ImageUrl { get; set; }
    }
}
