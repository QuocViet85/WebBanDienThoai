using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product 
{     
    [Table("ProductAttributeValue")]
    public class ProductAttributeValue
    {
        public int ProductId {set; get;}
        public int AttributeId {set; get;}
        public string? AttributeValue {set; get;}

        [ForeignKey("ProductId")]
        public ProductModel Product {set; get;}

        [ForeignKey("AttributeId")]
        public ProductAttribute ProductAttribute {set; get;}
    }
}