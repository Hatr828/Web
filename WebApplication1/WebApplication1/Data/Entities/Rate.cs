using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public class Rate
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid ProductId { get; set; }
        public int? Rating { get; set; }
        public String? Comment { get; set; }
        public DateTime Moment { get; set; }

        [JsonIgnore]
        public User User { get; set; }

        [JsonIgnore]
        public Product Product { get; set; }

    }
}
