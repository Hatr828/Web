using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public String Name { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;
        public String? ImagesCsv { get; set; }
        public String Slug { get; set; } = null!;

        [JsonIgnore]
        public IEnumerable<Product> Products { get; set; } = [];
    }
}
