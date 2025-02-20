using WebApplication1.Data.Entities;

namespace WebApplication1.Models
{
    public class ShopIndexPageModel
    {
        public Category[] Categories { get; set; } = [];
        public Dictionary<String, String>? Errors { get; set; }
        public bool? ValidationStatus { get; set; }
    }
}
