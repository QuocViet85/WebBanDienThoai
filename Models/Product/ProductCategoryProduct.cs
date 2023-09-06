using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product
{  
    [Table("ProductCategoryProduct")]
    public class ProductCategoryProduct
    {
        public int ProductId {set; get;}
        public int CategoryProductId {set; get;}

        [ForeignKey("ProductId")]
        public ProductModel Product {set; get;}
        
        [ForeignKey("CategoryProductId")]
        public CategoryProduct CategoryProduct {set; get;}

        
    }
}