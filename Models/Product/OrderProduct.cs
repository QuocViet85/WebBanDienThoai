using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product
{   
    //Đây là bảng trung gian của 2 bảng có mối quan hệ nhiều - nhiều là Order và ProductModel
    [Table("OrderProduct")]
    public class OrderProduct
    {
        public int ProductId {set; get;}
        public int OrderId {set; get;}
        public int Quantity {set; get;}

        [ForeignKey("ProductId")]
        public ProductModel Product {set; get;}

        [ForeignKey("OrderId")]
        public Order Order {set; get;}
    }
}