using System.ComponentModel.DataAnnotations;

namespace CodeLeap.Application.DTOs.Product
{
    public class UpdateProductRequest
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Description { get; set; }

        [Required]
        [Url]
        public required string ImageUrl { get; set; }
    }
}
