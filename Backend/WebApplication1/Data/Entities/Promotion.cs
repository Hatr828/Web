using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public class Promotion
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal DiscountPercentage { get; set; }
        [JsonIgnore]
        public List<Product> Products { get; set; } = [];
    }
}
