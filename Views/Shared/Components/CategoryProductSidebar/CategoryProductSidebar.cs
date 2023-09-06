using App.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
    [ViewComponent]
    public class CategoryProductSidebar : ViewComponent
    {

        public class CategorySidebarData
        {
            public List<CategoryProduct> Categories {set; get;}
            public int level {set; get;}
            public string? categoryslug {set; get;}
        }

        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
    }
}