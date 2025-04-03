using WebApplication1.Data.Entities;

namespace WebApplication1.Models.Shop
{
    public class ShopProductPageModel
    {
        public Product? Product { get; set; }
        public Category[] Categories { get; set; } = [];
        public bool CanUserRate { get; set; }
        public Rate? UserRate { get; set; }
        public string? AuthUserId { get; set; }
        public bool IsUserCanRate { get; internal set; }
    }
}
