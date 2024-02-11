using ShopApp.Entity;

namespace ShopApp.WebUI.Models
{
    public class ProductDetailModel
    {
        public Product Product { get; set; }
        public List<Category>? Categories { get; set; }
    }
}
