using Microsoft.EntityFrameworkCore;
using WebApplication1.Data.DBContexts;
using WebApplication1.Models.Shop;

namespace WebApplication1.Data
{
    public class DataAccessor(ApplicationDbContext dataContext)
    {
        private readonly ApplicationDbContext _dataContext = dataContext;
        public ShopIndexPageModel CategoriesList()
        {
            ShopIndexPageModel model = new()
            {
                Categories = [.. _dataContext.Categories]
            };
            return model;
        }
        public ShopCategoryPageModel CategoryById(string id)
        {
            ShopCategoryPageModel model = new()
            {
                Category = _dataContext
                .Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Rates)
                .FirstOrDefault(c => c.Slug == id),
                Categories = [.. _dataContext.Categories]
            };
            return model;
        }
    }
}
