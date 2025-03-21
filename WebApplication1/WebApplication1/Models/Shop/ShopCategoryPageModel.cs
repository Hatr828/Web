using WebApplication1.Data.Entities;

namespace WebApplication1.Models.Shop
{
    public class ShopCategoryPageModel
    {
        public Data.Entities.Category? Category { get; set; }
        public Category[] Categories { get; set; } = [];
    }
}
