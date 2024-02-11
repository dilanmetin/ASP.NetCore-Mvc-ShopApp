using ShopApp.Entity;
using System.ComponentModel.DataAnnotations;

namespace ShopApp.WebUI.Models
{
    public class ProductModel
    {
        public int ProductId { get; set; }

        //[Display(Name="Name",Prompt ="Enter Product Name")]
        //[Required(ErrorMessage ="Name zorunludur")]
        //[StringLength(60,MinimumLength =5,ErrorMessage ="Ürün ismi 5-10 karakter aralığında olmalıdır")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "URL zorunludur")]
        public string Url { get; set; }

        //[Required(ErrorMessage = "Price zorunludur")]
        //[Range(1,100000, ErrorMessage ="Price için 1-100000 aralığında değer giriniz")]
        public double? Price { get; set; }

        [Required(ErrorMessage = "Description zorunludur")]
        [StringLength(6000, MinimumLength = 5, ErrorMessage = "Description 5-6000 karakter aralığında olmalıdır")]

        public string Description { get; set; }

        [Required(ErrorMessage = "ImageUrl zorunludur")]
        public string ImageUrl { get; set; }
        public bool IsApproved { get; set; }
        public bool IsHome { get; set; }
        
        public List<Category>? SelectedCategories { get; set; }
    }
}
