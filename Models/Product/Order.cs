using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product
{
    public class Order
    {
        [Key]
        public int OrderId {set; get;}

        [Display(Name = "Tên khách hàng")]
        public string? CustomeName {set; get;}

        [Display(Name = "Địa chỉ nhận hàng")]
        [Required(ErrorMessage = "Phải nhập địa chỉ nhận hàng")]
        public string Address {set; get;}  

        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Phải nhập số điện thoại")]
        public int PhoneNumber {set; get;}

        [EmailAddress(ErrorMessage = "Nhập sai định dạng Email")]
        public string? Email {set; get;}
        public string? UserId {set; get;}

        // [ForeignKey("UserId")] //lấy trường dữ liệu nào trong bảng Order này để làm ForeignKey
        public AppUser? user {set; get;}

        [Display(Name = "Trạng thái đơn hàng")]
        public bool Completed {set; get;} = false;

        [Display(Name = "Thời điểm đặt hàng")]
        public DateTime TimeOrder {set; get;}
        public List<OrderProduct>? OrderProducts {set; get;}

        public List<Voucher>? Vouchers {set; get;}
        
        [Display(Name = "Tổng tiền")]
        public double Total {set; get;}
    }
}