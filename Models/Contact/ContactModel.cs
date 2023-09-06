using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Contacts
{
    public class ContactModel
    {
        [Key]
        public int Id {set; get;}

        [Column(TypeName = "nvarchar")]
        [StringLength(50)]
        [Required(ErrorMessage = "Phải nhập họ tên")]
        [Display(Name = "Họ tên")]
        public string FullName {set; get;}

        [EmailAddress(ErrorMessage = "Phải là địa chỉ Email")]
        [Required(ErrorMessage = "Phải nhập Email")]
        [StringLength(100)]
        [Display(Name = "Địa chỉ Email")]
        public string Email {set; get;}
        public DateTime? DateSent {set; get;}

        [Display(Name = "Nội dung")]
        public string? Message {set; get;}

        [StringLength(50)]
        [Phone(ErrorMessage = "Phải là số điện thoại")]
        [Display(Name = "Số điện thoại")]
        public string Phone {set; get;}
    }
}