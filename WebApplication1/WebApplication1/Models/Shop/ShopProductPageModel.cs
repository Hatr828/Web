using WebApplication1.Data.Entities;

namespace WebApplication1.Models.Shop
{
    public class ShopProductPageModel
    {
        public Data.Entities.Product? Product { get; set; }
        public Category[] Categories { get; set; } = [];
    }
}
