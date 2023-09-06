using System.ComponentModel.DataAnnotations;
using App.Models.Product;
using Bogus.DataSets;

namespace App.Areas.Product.Model
{
    public class CreateProductModel : ProductModel
    {
        [Display(Name = "Danh mục sản phẩm")]
        [Required(ErrorMessage = "Phải chọn danh mục sản phẩm")]
        public int[]? categoryIds {set; get;}
    }
}