using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Authorization;
using App.Data;

namespace AppMvc.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/categoryproduct/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Moderator)]
    public class CategoryProductController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryProductController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)  // cùng lấy ra category cha khi lấy ra mỗi category
                    .Include(c => c.CategoryChildren); // cùng lấy ra category con khi lấy ra mỗi category

            var categories = (await qr.ToListAsync()).Where(c => c.ParentCategory == null).ToList(); //lấy ra các category không có category cha
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        private void CreatedSelectItems(List<CategoryProduct> source, List<CategoryProduct> des, int level)
        {
            var prefixEnumerable = Enumerable.Repeat("--", level);
            string prefix = string.Concat(prefixEnumerable);
            foreach (var category in source)
            {
                des.Add(new CategoryProduct(){
                    Id = category.Id,
                    Title = prefix + " " + category.Title
                });
                if (category.CategoryChildren?.Count > 0)
                {
                    CreatedSelectItems(category.CategoryChildren.ToList(), des, level + 1);
                }
                
            }
        }

        // GET: Category/Create
        public async Task<IActionResult> Create()
        {
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.ParentCategory);

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new CategoryProduct() {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<CategoryProduct>();

            CreatedSelectItems(categories, items, 0);

            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title");
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] CategoryProduct category)
        {
            foreach (var modelstateKey in ModelState.Keys)
            {
                var modelstateVal = ModelState[modelstateKey];
                string keyError = null;
                foreach (var error in modelstateVal.Errors)
                {
                    keyError = modelstateKey;
                    var er = error.ErrorMessage;
                    Console.WriteLine($"{keyError} - {er}");
                }
            }

            if (ModelState.IsValid)
            {
                if (category.ParentCategoryId == -1)
                {
                    category.ParentCategoryId = null;
                }

                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.ParentCategory);

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new CategoryProduct() {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<CategoryProduct>();

            CreatedSelectItems(categories, items, 0);

            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title");
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.ParentCategory);

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new CategoryProduct() {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<CategoryProduct>();

            CreatedSelectItems(categories, items, 0);

            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title");
            return View(category);
        }

        // private bool checkCategoryChildrens(Category category)
        // {
        //     var childrenCategories = (from c in _context.Categories select c)
        //                                 .Include(c => c.CategoryChildren)
        //                                 .ToList()
        //                                 .Where(c => c.ParentCategoryId == category.Id);
        //         if (childrenCategories != null)
        //         {
        //             foreach (var childrenCategory in childrenCategories)
        //             {
        //                 if (category.ParentCategoryId == childrenCategory.Id)
        //                 {
        //                     return false;
        //                 }
        //                 if (childrenCategory.CategoryChildren != null)
        //                 {
        //                     return checkCategoryChildrens(childrenCategory);
        //                 }
        //             }
        //         }
                
        //         return true;
        // }
        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] CategoryProduct category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            bool canUpdate = true;

            if (category.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
                canUpdate = false;
            }

            // Kiem tra thiet lap muc cha phu hop
            if (canUpdate && category.ParentCategoryId != null)
            { 
            var childCates =  
                        (from c in _context.CategoryProducts select c)
                        .Include(c => c.CategoryChildren)
                        .ToList()
                        .Where(c => c.ParentCategoryId == category.Id);


                // Func check Id 
                Func<List<CategoryProduct>, bool> checkCateIds = null;
                checkCateIds = (cates) => 
                    {
                        foreach (var cate in cates)
                        { 
                             Console.WriteLine(cate.Title); 
                            if (cate.Id == category.ParentCategoryId)
                            {
                                canUpdate = false;
                                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khácXX");
                                return true;
                            }
                            if (cate.CategoryChildren!=null)
                                return checkCateIds(cate.CategoryChildren.ToList());
                        }
                        return false;
                    };
                // End Func 
                checkCateIds(childCates.ToList()); 
            }

            if (ModelState.IsValid && canUpdate)
            {
                try
                {
                    if (category.ParentCategoryId == -1)
                    {
                        category.ParentCategoryId = null;
                    }
                    var categoryFromDatabase = _context.CategoryProducts.FirstOrDefault(c => c.Id ==id);

                    _context.Entry(categoryFromDatabase).State = EntityState.Detached; // thiết lập để DbContext không theo dõi đối tượng cần Edit trong DbContext nữa để update đối tượng này bằng đối tượng được binding vào sau khi Edit vào DbContext không bị lỗi

                    _context.CategoryProducts.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            var qr = (from c in _context.CategoryProducts select c)
                    .Include(c => c.ParentCategory)
                    .Include(c => c.ParentCategory);

            var categories = (await qr.ToListAsync())
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            categories.Insert(0, new CategoryProduct() {
                Id = -1,
                Title = "Không có danh mục cha"
            });

            var items = new List<CategoryProduct>();

            CreatedSelectItems(categories, items, 0);

            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title");
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.CategoryProducts
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.CategoryProducts
                            .Include(c => c.CategoryChildren)
                            .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return NotFound();
            }
            else 
            {
                foreach (var childrenCategory in category.CategoryChildren)
                {
                    childrenCategory.ParentCategoryId = category.ParentCategoryId;
                }
            }

            _context.CategoryProducts.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.CategoryProducts.Any(e => e.Id == id);
        }
    }
}
