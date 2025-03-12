using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;

        public String? Phone { get; set; }
        public String? WorkPosition { get; set; }
        public String? PhotoUrl { get; set; }

        public String Slug { get; set; } = null!; 


        [JsonIgnore]
        public List<UserAccess> Accesses { get; set; } = [];

        [JsonIgnore]
        public List<Cart> Carts { get; set; } = [];

        [JsonIgnore]
        public List<Rate>? Rates { get; set; }
    }
}