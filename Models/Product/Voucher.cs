using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product 
{
    [Table("Voucher")]
    public class Voucher 
    {  
        [Key]
        public int VoucherId {set; get;}

        [Display(Name = "Giảm giá")]
        [Range(0, 100)]
        public int PercentageDiscount {set; get;}
        public string UserId {set; get;}
        public int? OrderId {set; get;}
        public AppUser User {set; get;}
        public Order? Order {set; get;}

        [DataType(DataType.Date)]
        public DateTime LastDrawDate {set; get;}

        [DataType(DataType.Date)]
        public DateTime ExpiredDate {set; get;} 

    }
}