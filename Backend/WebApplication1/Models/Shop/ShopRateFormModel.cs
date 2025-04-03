namespace WebApplication1.Models.Shop
{
    public class ShopRateFormModel
    {
        public String UserId { get; set; } = null!;
        public String ProductId { get; set; } = null!;
        public String? Comment { get; set; }
        public int? Rating { get; set; }

    }
}
