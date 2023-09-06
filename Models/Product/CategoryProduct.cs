using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Product
{
    [Table("CategoryProduct")]
    public class CategoryProduct
    {
        [Key]
        public int Id {set; get;}

        [Required(ErrorMessage = "Phải có tên danh mục")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên danh mục")]
        public string Title {set; get;}

        [DataType(DataType.Text)]
        [Display(Name = "Nội dung danh mục")]
        public string? Description {set; get;}

        [Required(ErrorMessage = "Phải tạo url")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        [Display(Name = "Url hiện thị")]

        public string? Slug {set; get;}

        public ICollection<CategoryProduct>? CategoryChildren {set; get;}

        [Display(Name = "Danh mục cha")]
        public int? ParentCategoryId {set; get;}

        [ForeignKey("ParentCategoryId")]
        [Display(Name = "Danh mục cha")]
        public CategoryProduct? ParentCategory {set; get;}

        public void ChildCategoryIDs(List<int> lists, ICollection<CategoryProduct> childcates = null)
        {
            if (childcates == null)
            {
                childcates = this.CategoryChildren;
            }

            foreach (CategoryProduct categoryProduct in childcates)
            {
                lists.Add(categoryProduct.Id);
                ChildCategoryIDs(lists, categoryProduct.CategoryChildren);
            }
        }

        public List<CategoryProduct> ListParents()
        {
            List<CategoryProduct> lists = new List<CategoryProduct>();
            var parent = this.ParentCategory;

            while (parent != null)
            {
                lists.Add(parent);
                parent = parent.ParentCategory;
            }

            return lists;
        }
    }
}