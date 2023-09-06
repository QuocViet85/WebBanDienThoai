using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using App.Models.Product;
using Microsoft.AspNetCore.Identity;

namespace App.Models 
{
    public class AppUser: IdentityUser 
    {
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]  
        public string? HomeAddress { get; set; }

        // [Required]       
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        public List<Order>? Orders {set; get;}

        public List<Voucher>? Vouchers {set; get;}
    }
}
