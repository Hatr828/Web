using WebApplication1.Data.Entities;

namespace WebApplication1.Models.User
{
    public class UserProfilePageModel
    {
        public bool IsFound { get; set; }
        public String Name { get; set; } = "";
        public String Email { get; set; } = "";
        public String Phone { get; set; } = "";
        public String MostViewed { get; set; } = "";
        public String Recent { get; set; } = "";
        public String Role { get; set; } = "";
        public String PhotoUrl { get; set; } = "";

        public bool IsOwner { get; set; }
        public List<Cart> Carts { get; set; } = [];
    }
}
