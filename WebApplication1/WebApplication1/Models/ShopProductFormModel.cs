namespace WebApplication1.Models
{
    public class ShopProductFormModel
    {
        public String Name { get; set; }
        public String Slug { get; set; }
        public String Description { get; set; }
        public Guid CategoryId { get; set; }
        public int Stock { get; set; }
        public decimal Price { get; set; }
        public IFormFile[] Images { get; set; }
    }
}
