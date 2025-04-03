using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public record Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImagesCsv { get; set; }
        public string Slug { get; set; } = null!;

        [JsonIgnore]
        public List<Product> Products { get; init; } = new();
    }
}
