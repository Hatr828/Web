using System.Text.Json.Serialization;

namespace WebApplication1.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;

        [JsonIgnore]
        public List<UserAccess> Accesses { get; set; } = [];
    }
}