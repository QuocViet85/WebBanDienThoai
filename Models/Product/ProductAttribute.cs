using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product 
{
    [Table("ProductAttribute")]
    public class ProductAttribute
    {
        [Key]
        public int AttributeId {set; get;}

        [Display(Name = "Tên thuộc tính")]
        public string? AttributeName {set; get;}

        [Display(Name = "Thứ tự ưu tiên")]
        [Required]
        public int Sort {set; get;}
    }
}