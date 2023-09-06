using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product 
{
    [Table("Product")]
    public class ProductModel 
    {
            [Key] 
            public int ProductID {set; get;}

            [Required(ErrorMessage = "Phải có tiêu đề sản phẩm")]
            [Display(Name = "Tiêu đề")]
            [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {2} đến {1}")]
            public string Title {set; get;}

            [Display(Name = "Mô tả")]
            public string? Description {set; get;}

            [Display(Name = "Số lượng")]
            public int? Quantity {set; get;}

            [Display(Name="Chuỗi định danh (url)", Prompt = "Nhập hoặc để trống tự phát sinh theo Title")]
            [StringLength(160, MinimumLength = 5, ErrorMessage = "{0} dài {1} đến {2}")]
            [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
            [Required(ErrorMessage = "Phải nhập chuỗi Url cho sản phẩm")]
            public string? Slug {set; get;}

            [Display(Name = "Nội dung")]
            public string? Content {set; get;}

            // [Required]
            [Display(Name = "Người đăng")]
            public string? AuthorId {set; get;}
            [ForeignKey("AuthorId")]
            [Display(Name = "Người đăng")]
            public AppUser? Author {set; get;}

            [Display(Name = "Ngày tạo")]
            public DateTime? DateCreated {set; get;}

            [Display(Name = "Ngày cập nhật")]
            public DateTime? DateUpdated {set; get;}

            [Display(Name = "Giá sản phẩm")]
            [Range(0, int.MaxValue, ErrorMessage="Nhập giá trị từ {1}")]
            public int? Price {set; get;}

            [Display(Name = "Thời hạn bảo hành")]
            public double GuaranteePeriod {set; get;}

            public List<ProductAttributeValue>? ProductAttributeValues {set; get;}
        
            public List<ProductCategoryProduct>?  ProductCategoryProducts { get; set; }

            public List<ProductPhoto>? Photos {set; get;}

            public List<OrderProduct>? OrderProducts {set; get;}

            public void SortProductAttribute()
            {
                this.ProductAttributeValues.Sort((pa1, pa2) => 
                {
                    if (pa1.ProductAttribute.Sort > pa2.ProductAttribute.Sort)
                    {
                        return 1;
                    }
                    else if (pa1.ProductAttribute.Sort == pa2.ProductAttribute.Sort)
                    {
                        return 0;
                    }
                    else 
                    {
                        return -1;
                }
                });
            }

    }
}