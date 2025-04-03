using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public record Product
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImagesCsv { get; set; }
        public string? Slug { get; set; }
        [JsonIgnore]
        public Category Category { get; set; } = null!;
        //[JsonIgnore]
        public List<Rate> Rates { get; set; } = [];
        [JsonIgnore]
        public List<Promotion> Promotions { get; set; }
    }
}
