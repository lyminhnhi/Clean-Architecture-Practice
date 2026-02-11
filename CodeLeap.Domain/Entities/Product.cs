using CodeLeap.Domain.Common;

namespace CodeLeap.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public Product() { }

        public Product(string name, string description, string imageUrl)
        {
            Name = name;
            Description = description;
            ImageUrl = imageUrl;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Name);
        }
    }
}
