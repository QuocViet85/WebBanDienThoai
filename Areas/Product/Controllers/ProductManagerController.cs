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
using App.Areas.Product.Model;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Text;

namespace AppMvc.Areas.Product.Controllers
{
    [Area("Product")]
    [Route("admin/product/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator + "," + RoleName.Moderator)]
    public class ProductManagerController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        private readonly ILogger<ProductManagerController> _logger;

        public ProductManagerController(AppDbContext context, UserManager<AppUser> userManager, ILogger<ProductManagerController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [TempData]
        public string StatusMessage { set; get; }

        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize, string? search)
        {
            IOrderedQueryable<ProductModel> products = null;
            int totalProducts = 0;
            if (string.IsNullOrEmpty(search))
            {
                products = _context.Products
                    .Include(p => p.Author)
                    .OrderByDescending(p => p.DateUpdated);
            
                    totalProducts = products.Count();
                    ViewBag.Search = false;
            }
            else
            {
                products = _context.Products
                        .Where(p => p.Title.Contains(search) || p.Description.Contains(search))
                        .Include(p => p.Author)
                        .OrderByDescending(p => p.DateUpdated);
                
                totalProducts = products.Count();
                ViewBag.Search = true;
                ViewBag.KeyWord = search;
            }

            if (pagesize <= 0)
            {
                pagesize = 6;
            }
            int countPages = (int)Math.Ceiling((double)totalProducts / pagesize);

            if (currentPage > countPages)
            {
                currentPage = countPages;
            }
            if (currentPage < 1)
            {
                currentPage = 1;
            }

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize = pagesize
                })
            };

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalProducts = totalProducts;
            ViewBag.productIndex = (currentPage - 1) * pagesize;

            var productsInPage = await products.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize)
                        .Include(p => p.ProductCategoryProducts)
                        .ThenInclude(pc => pc.CategoryProduct)
                        .ToListAsync();

            return View(productsInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .Include(p => p.ProductAttributeValues)
                .ThenInclude(pa => pa.ProductAttribute)
                .FirstOrDefaultAsync(m => m.ProductID == id);
            
            if (product == null)
            {
                return NotFound();
            }

            product.SortProductAttribute();

            

            return View(product);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            var categoryProducts = await _context.CategoryProducts.ToListAsync();

            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");
            ViewBag.ProductAttribute = _context.ProductAttributes.OrderBy(pa => pa.Sort).ToList();
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Price,Quantity,GuaranteePeriod,Description,Slug,Content,categoryIds")] CreateProductModel product, IFormFile? FileUpload)
        {
            var categoryProducts = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");
            ViewBag.ProductAttribute = _context.ProductAttributes.OrderBy(pa => pa.Sort).ToList();

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi Url khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                product.DateCreated = product.DateCreated = DateTime.Now;
                product.AuthorId = user.Id;
                _context.Add(product); //đến đây Entity Framework tự động gán giá trị cho thuộc tính Primary key

                if (product.categoryIds != null)
                {
                    foreach (var CateId in product.categoryIds)
                    {
                        _context.Add(new ProductCategoryProduct()
                        {
                            CategoryProductId = CateId,
                            Product = product
                        });
                    }
                }
                
                await _context.SaveChangesAsync(); //Đã lưu xong sản phẩm lên database. 

                var productAttributes = _context.ProductAttributes.ToList();
                var productAttributeNames = productAttributes.Select(pa => pa.AttributeName);
                var keys = Request.Form.Keys;
                var keysAttribute =  (from key in keys
                                    join name in productAttributeNames
                                    on key equals name
                                    select key).ToList();
                
                if (keysAttribute?.Count > 0)
                {
                    foreach (var key in keysAttribute)
                    {
                        var attributeValue = Request.Form[key];
                        if (!string.IsNullOrEmpty(attributeValue.ToString()))
                        {
                            _context.ProductAttributeValues.Add(new ProductAttributeValue()
                            {
                                ProductId = product.ProductID,
                                AttributeId = productAttributes.Where(pa => pa.AttributeName == key).Select(pa => pa.AttributeId).FirstOrDefault(),
                                AttributeValue = attributeValue.ToString()
                            });
                        }
                        
                    }
                    _context.SaveChanges();
                }

                if (FileUpload != null)
                {
                    string[] ExtensionFileUpload = {".png",".jpg",".jpeg",".gif"};

                    if (ExtensionFileUpload.Contains(Path.GetExtension(FileUpload.FileName)))
                    {
                        var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(FileUpload.FileName);

                        var file = Path.Combine("Uploads", "Products", file1);

                        using (FileStream fileStream = new FileStream(file, FileMode.Create))
                        {
                            await FileUpload.CopyToAsync(fileStream);
                        }

                        _context.ProductPhotos.Add(new ProductPhoto() {
                            FileName = file1,
                            ProductID = product.ProductID
                        });

                        _context.SaveChanges();
                    }
                    else
                    {
                        StatusMessage += $"File hình ảnh có đuôi {Path.GetExtension(FileUpload.FileName)} là không phù hợp. Sản phẩm vẫn được tạo nhưng không có hình ảnh";
                    } 
                }

                if (string.IsNullOrEmpty(StatusMessage))
                {
                    StatusMessage += "Vừa tạo sản phẩm mới";
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Post/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                        .Include(p => p.ProductCategoryProducts)
                        .Include(p => p.ProductAttributeValues)
                        .ThenInclude(pa => pa.ProductAttribute)
                        .FirstOrDefaultAsync(p => p.ProductID == id);

            if (product == null)
            {
                return NotFound();
            }
            
            ViewBag.ProductAttribute = _context.ProductAttributes.OrderBy(pa => pa.Sort).ToList();

            var productEdit = new CreateProductModel()
            {
                ProductID = product.ProductID,
                Title = product.Title,
                Price = product.Price,
                Quantity = product.Quantity,
                GuaranteePeriod = product.GuaranteePeriod,
                DateCreated = product.DateCreated,
                Content = product.Content,
                Description = product.Description,
                Slug = product.Slug,
                categoryIds = product.ProductCategoryProducts.Select(pc => pc.CategoryProductId).ToArray<int>(),
                ProductAttributeValues = product.ProductAttributeValues
            };

            var categoryProducts = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");

            return View(productEdit);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductID,Title,Price,Quantity,GuaranteePeriod,Description,DateCreated,Slug,Content,categoryIds")] CreateProductModel product) 
        {
        /*Vẫn bind được tham số Id vì trong file Edit.cshtml thiết lập giá trị thuộc tính action cho thẻ form thông qua TagHelper với action khai báo với TagHelper là Edit trùng tên với file Edit.cshtml nên TagHelper thiết lập giá trị cho thuộc tính action của thẻ form là Url ban đầu dẫn đến file Edit.cshtml (chính là Url dẫn đến Action Edit truy cập bằng phương thức GET, Route trong URL đó có tham số là id cúa sản phẩm) => Action Edit truy cập bằng phương thức POST này vẫn có thể bind tham số id từ Route trong URL của request chứa form gửi đến (chú ý là action Edit truy cập bằng phương thức POST này phải có cùng Route với action Edit truy cập bằng phương thức GET nếu không request chứa form sẽ không thể gửi đến action Edit truy cập bằng phương thức POST này)
        */
            if (id != product.ProductID)
            {
                return NotFound();
            }

            var categoryProducts = await _context.CategoryProducts.ToListAsync();
            ViewData["categoryProducts"] = new MultiSelectList(categoryProducts, "Id", "Title");
            ViewBag.ProductAttribute = _context.ProductAttributes.OrderBy(pa => pa.Sort).ToList();

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductID != id))
            {
                ModelState.AddModelError("Slug", "Nhập chuỗi Url khác");
                return View(product);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productUpdate = _context.Products
                                    .Include(p => p.ProductCategoryProducts)
                                    .Include(p => p.ProductAttributeValues)
                                    .ThenInclude(pav => pav.ProductAttribute)
                                    .FirstOrDefault(p => p.ProductID == id);

                    if (productUpdate == null)
                    {
                        return NotFound();
                    }

                    productUpdate.Title = product.Title;
                    productUpdate.Price = product.Price;
                    productUpdate.Quantity = product.Quantity;
                    productUpdate.GuaranteePeriod = product.GuaranteePeriod;
                    productUpdate.Description = product.Description;
                    productUpdate.Content = product.Content;
                    productUpdate.Slug = product.Slug;
                    productUpdate.DateUpdated = DateTime.Now;

                    if (product.DateCreated != null)
                    {
                        productUpdate.DateCreated = product.DateCreated;
                    }
                    //Update PostCategory
                    var oldCateProducts = productUpdate.ProductCategoryProducts.ToArray();
                    if (oldCateProducts != null)
                    {
                        _context.ProductCategoryProducts.RemoveRange(oldCateProducts);
                    }
                    var newCategoryIds = product.categoryIds;
                    if (newCategoryIds != null)
                    {
                        foreach (var categoryId in newCategoryIds)
                        {
                            _context.ProductCategoryProducts.Add(new ProductCategoryProduct()
                            {
                                CategoryProductId = categoryId,
                                ProductId = id
                            });
                        }
                    }

                    _context.Update(productUpdate);
                    await _context.SaveChangesAsync();

                var productAttributes = _context.ProductAttributes.ToList();
                var productAttributeNames = productAttributes.Select(pa => pa.AttributeName);
                var keys = Request.Form.Keys;
                var keysAttribute =  (from key in keys
                                    join name in productAttributeNames
                                    on key equals name
                                    select key).ToList();

                var productAttributeValueOfThisProduct = _context.ProductAttributeValues.Where(pav => pav.ProductId == product.ProductID).Include(pav => pav.ProductAttribute).ToList(); 
                
                if (keysAttribute?.Count > 0)
                {
                    foreach (var key in keysAttribute)
                    {
                        var attributeValue = Request.Form[key];
                        if (!string.IsNullOrEmpty(attributeValue.ToString()))
                        {
                            if (productAttributeValueOfThisProduct.Any(pav => pav.ProductAttribute.AttributeName == key)) //kiểm tra sản phẩm này đã có attribute nào rồi
                            {
                                try 
                                {
                                    productAttributeValueOfThisProduct.Where(pav => pav.ProductAttribute.AttributeName == key).FirstOrDefault().AttributeValue = attributeValue;
                                }
                                catch (Exception e)
                                {
                                    return NotFound();
                                } 
                            }
                            else 
                            {
                                _context.ProductAttributeValues.Add(new ProductAttributeValue()
                                {
                                    ProductId = product.ProductID,
                                    AttributeId = productAttributes.Where(pa => pa.AttributeName == key).Select(pa => pa.AttributeId).FirstOrDefault(),
                                    AttributeValue = attributeValue.ToString()
                                });
                            }
                        } 
                    }
                    _context.SaveChanges();
                }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                StatusMessage = "Vừa cập nhật sản phẩm";
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
            return View(product);
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.ProductID == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            StatusMessage = "Bạn vừa xóa sản phẩm: " + product.Title;
            return RedirectToAction(nameof(Index));
        }

        
        public async Task<IActionResult> ProductAttributeManage()
        {
            var productAttributes = _context.ProductAttributes.OrderBy(pa => pa.Sort).ToList();
            return View(productAttributes);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProductAttribute()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductAttribute([Bind("AttributeName, Sort")] ProductAttribute productAttribute)
        {
            if (!string.IsNullOrEmpty(productAttribute.AttributeName))
            {
                if (_context.ProductAttributes.Any(pa => pa.AttributeName == productAttribute.AttributeName))
                {
                    ModelState.AddModelError(string.Empty, "Thuộc tính này đã tồn tại, hãy chọn thuộc tính khác");
                    return View();
                }

                await _context.ProductAttributes.AddAsync(productAttribute);
                await _context.SaveChangesAsync();
                StatusMessage = $"Đã thêm thuộc tính {productAttribute.AttributeName} thành công";
                return RedirectToAction(nameof(ProductAttributeManage));
            }
            return View();
        }   

        [HttpGet]
        public async Task<IActionResult> EditProductAttribute(int id)
        {
            var productAttribute = _context.ProductAttributes.Find(id);
            if (productAttribute == null)
            {
                return NotFound();
            }
            
            return View(productAttribute);
        }

        [HttpPost]
        public async Task<IActionResult> EditProductAttribute(int id, [Bind("AttributeId, AttributeName, Sort")] ProductAttribute productAttribute) //gửi Request bao gồm cả Route từ trang gửi đi nên bind được tham số id từ Route
        {
            if (id != productAttribute.AttributeId)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(productAttribute.AttributeName))
            {
                if (_context.ProductAttributes.Any(pa => pa.AttributeName == productAttribute.AttributeName && pa.AttributeId != productAttribute.AttributeId))
                {
                    ModelState.AddModelError(string.Empty, "Thuộc tính này đã tồn tại, hãy chọn thuộc tính khác");
                    return View();
                }

                var productAttributeEdit = await _context.ProductAttributes.FindAsync(id);
                productAttributeEdit.AttributeName = productAttribute.AttributeName;
                productAttributeEdit.Sort = productAttribute.Sort;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ProductAttributeManage));
            }
            return View();
        }

        public async Task<IActionResult> DeleteProductAttribute(int AttributeId)
        {
            var productAttribute = await _context.ProductAttributes.FindAsync(AttributeId);
            
            if (productAttribute == null)
            {
                return NotFound();
            }

            _context.ProductAttributes.Remove(productAttribute);
            _context.SaveChanges();
            
            return RedirectToAction(nameof(ProductAttributeManage));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.ProductID == id);
        }

        public class UploadOneFile
        {
            [Required(ErrorMessage = "Phải chọn file để upload")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "png,jpg,jpeg,gif")]
            [Display(Name = "Chọn file upload")]
            public IFormFile FileUpload {set; get;}
        }

        [HttpGet]
        public IActionResult UploadPhoto(int? id)
        {
            var product = _context.Products.Where(p => p.ProductID == id)
                            .Include(p => p.Photos)
                            .FirstOrDefault();

            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            ViewData["product"] = product;
            return View(new UploadOneFile());
        }

        [HttpPost, ActionName("UploadPhoto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhotoAsync(int? id, [Bind("FileUpload")]UploadOneFile f)
        {
            var product = _context.Products.Where(p => p.ProductID == id)
                        .Include(p => p.Photos)
                        .FirstOrDefault();
            
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }
            ViewData["product"] = product;

            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                            + Path.GetExtension(f.FileUpload.FileName); //tạo ra tên file ngẫu nhiên để không bị trùng với tên file đã có
                
                var file = Path.Combine("Uploads", "Products", file1); //tạo đường dẫn lưu file

                using (var filestream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(filestream);
                }

                _context.Add(new ProductPhoto()
                {
                    ProductID = product.ProductID,
                    FileName = file1,
                });  

                await _context.SaveChangesAsync();
            }
            return View(f);
        }

        [HttpPost]
        public IActionResult ListPhotos(int id)
        {
            var product = _context.Products.Where(p => p.ProductID == id)
                        .Include(p => p.Photos)
                        .FirstOrDefault();

            if (product == null)
            {
                return Json(
                    new {
                        success = 0,
                        message = "Product not found"
                    }
                );
            }

            var listPhotos = product.Photos.Select(photo => new {
                id = photo.Id,
                path = "/contents/Products/" + photo.FileName
            });

            return Json(
                new {
                    success = 1,
                    photos = listPhotos
                }
            );
        }

        [HttpPost]
        public IActionResult DeletePhoto(int? id)
        {
            var photo = _context.ProductPhotos
                        .Where(photo => photo.Id == id)
                        .FirstOrDefault();
            
            if (photo != null)
            {
                _context.ProductPhotos.Remove(photo);
                _context.SaveChanges();

                var filename = "Uploads/Products/" + photo.FileName;
                System.IO.File.Delete(filename);
            }
            
            return Ok(); //trả về mã thành công
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotoApi(int id, [Bind("FileUpload")]UploadOneFile f)
        {
            var product = _context.Products
                        .Include(p => p.Photos)
                        .Where(p => p.ProductID == id)
                        .FirstOrDefault();
            
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            if (f != null)
            {
                var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(f.FileUpload.FileName);

                var file = Path.Combine("Uploads", "Products", file1);

                using (var fileStream = new FileStream(file, FileMode.Create))
                {
                    await f.FileUpload.CopyToAsync(fileStream);
                };

                _context.ProductPhotos.Add(new ProductPhoto(){
                    ProductID = product.ProductID,
                    FileName = file1
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> OrderManage()
        {
            List<Order> listOrders = _context.Orders
                                            .OrderByDescending(c => c.TimeOrder)
                                            .Include(o => o.user)
                                            .Include(o => o.OrderProducts)
                                            .ThenInclude(op => op.Product)
                                            .Include(o => o.Vouchers)
                                            .ToList();
            
            return View(listOrders);
        }

        [HttpPost]
        public async Task<IActionResult> OrderConfirm(int CartId, bool Confirmed)
        {
            Order order = _context.Orders
                                .Where(o => o.OrderId == CartId)
                                .Include(o => o.OrderProducts)
                                .ThenInclude(op => op.Product)
                                .FirstOrDefault();
            if (order == null)
            {
                return NotFound();
            }

            Console.WriteLine(Confirmed);

            if (Confirmed)
            {
                order.Completed = true;
                if (order.OrderProducts?.Count > 0)
                {
                    foreach (var orderProduct in order.OrderProducts)
                    {
                        orderProduct.Product.Quantity -= orderProduct.Quantity;
                    }
                }
            }
            else 
            {
                order.Completed = false;
                if (order.OrderProducts?.Count > 0)
                {
                    foreach (var orderProduct in order.OrderProducts)
                    {
                        orderProduct.Product.Quantity += orderProduct.Quantity;
                    }
                }
            }
            _context.SaveChanges();

            return RedirectToAction(nameof(OrderManage));
        }

        [HttpGet]
        public async Task<IActionResult> PrintBill(int CartId)
        {
            Order order = _context.Orders
                            .Where(o => o.OrderId == CartId)
                            .Include(o => o.user)
                            .Include(o => o.OrderProducts)
                            .ThenInclude(op => op.Product)
                            .Include(o => o.Vouchers)
                            .FirstOrDefault();
            
            if (order == null)
            {
                return NotFound();
            }

            string contentBill = $@"
                                        Quocvietmobile
                            Đ/c: 46A phố Phạm Ngọc Thạch, Đống Đa, Hà Nội
                                        HÓA ĐƠN BÁN HÀNG
                        ---------------------------------------------------
                        THÔNG TIN KHÁCH HÀNG:
                            Mã hóa đơn: {order.OrderId}
                            Tên khách hàng: {order.CustomeName}
                            Số điện thoại: {order.PhoneNumber}
                            Địa chỉ nhận hàng: {order.Address}
                        
                        THÔNG TIN SẢN PHẨM MUA:";
            int stt = 1;
            foreach (var orderProduct in order.OrderProducts)
            {
                contentBill += $@"
                            {stt}.
                            Sản phẩm: {orderProduct.Product.Title}
                            Số lượng: {orderProduct.Quantity}
                            Thành tiền: {string.Format(new CultureInfo("vi-VN"), "{0:#,##0}", orderProduct.Product.Price * orderProduct.Quantity)} VNĐ
                                ";
                stt++; 
            }

            contentBill += $@"
                        ---------------------------------------------------";

            if (order.Vouchers?.Count > 0)
            {
                double giamGia = 0;
                foreach (var voucher in order.Vouchers)
                {
                    giamGia += voucher.PercentageDiscount;
                }

                if (giamGia > 0)
                {
                    contentBill += $@"
                            Giám giá: {giamGia} %";
                }
            }

            contentBill += @$"
                            Tổng tiền: {string.Format(new CultureInfo("vi-VN"), "{0:#,##0}", order.Total)} VNĐ
                        ---------------------------------------------------
                        Chữ ký khách hàng (đã kiểm tra và nhận hàng)
                            
                            
                            

                        Thời gian xuất hóa đơn: {String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now)}";

            string fileName = $"{order.OrderId}.txt";

            string filePath = Path.Combine("Bills", fileName);

            using (var fileStream = new FileStream(path: filePath, mode: FileMode.Create, access: FileAccess.Write))
            {
                Byte[] byteBill = Encoding.UTF8.GetBytes(contentBill);
                await fileStream.WriteAsync(byteBill, 0, byteBill.Length);
            }

            return File(System.IO.File.ReadAllBytes(filePath), "text/plain", $"HoaDon{order.OrderId}.txt");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int CartId)
        {
            Order order = _context.Orders
                                .Where(c => c.OrderId == CartId)
                                .Include(c => c.Vouchers)
                                .FirstOrDefault();

            if (order == null)
            {
                return NotFound();
            }

            if (order.Vouchers != null)
            {
                foreach (var voucher in order.Vouchers)
                {
                    voucher.OrderId = null;
                }
                _context.SaveChanges();
            }

            _context.Remove(order);
            _context.SaveChanges();

            try 
            {
                System.IO.File.Delete(Path.Combine("Bills", $"{order.OrderId}.txt"));
            }
            catch{}

            return RedirectToAction(nameof(OrderManage));
        }

        public async Task<IActionResult> VoucherManage()
        {
            List<Voucher> listVouchers = _context.Vouchers
                                        .Where(v => v.PercentageDiscount != 0)
                                        .Include(v => v.User)
                                        .Include(v => v.Order)
                                        .ToList();
            return View(listVouchers);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVoucher(int VoucherId)
        {
            Voucher voucherDelete = _context.Vouchers
                                    .Where(v => v.VoucherId == VoucherId)
                                    .FirstOrDefault();

            if (voucherDelete == null)
            {
                return NotFound();
            }
            _context.Vouchers.Remove(voucherDelete);
            _context.SaveChanges();
            return RedirectToAction(nameof(VoucherManage));
        }

        [HttpGet]
        public async Task<IActionResult> OrderStatistics()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OrderStatistics(DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            ModelState.Clear();
            if (dateTimeFrom == new DateTime(0001, 1, 1) && dateTimeTo == new DateTime(0001, 1, 1))
            {
                ModelState.AddModelError(string.Empty, "Phải nhập dữ liệu cho ít nhất 1 mục Từ ngày hoặc Đến ngày");
                return View();
            }

            List<Order>? orderStatistics = null;

            if (dateTimeFrom == new DateTime(0001, 1, 1) || dateTimeTo == new DateTime(0001, 1, 1))
            {
                DateTime dateTimeSearch = dateTimeFrom != new DateTime(0001, 1, 1) ? dateTimeFrom : dateTimeTo;
                ViewBag.DateTimeFrom = ViewBag.DateTimeTo = $"{dateTimeSearch.Day}/{dateTimeSearch.Month}/{dateTimeSearch.Year}";

                orderStatistics = _context.Orders
                                        .Include(o => o.OrderProducts)
                                        .ThenInclude(op => op.Product)
                                        .Where(o => o.TimeOrder.Year == dateTimeSearch.Year && o.TimeOrder.Month == dateTimeSearch.Month && o.TimeOrder.Day == dateTimeSearch.Day)
                                        .ToList();
            }
            else
            {
                if (dateTimeFrom > dateTimeTo)
                {
                    ModelState.AddModelError(string.Empty, "Thời gian Từ ngày phải trước thời gian Đến ngày");
                    return View();
                }
                dateTimeTo = dateTimeTo.AddHours(23);
                dateTimeTo = dateTimeTo.AddMinutes(59);
                dateTimeTo = dateTimeTo.AddSeconds(59);

                ViewBag.DateTimeFrom = $"{dateTimeFrom.Day}/{dateTimeFrom.Month}/{dateTimeFrom.Year}";
                ViewBag.DateTimeTo = $"{dateTimeTo.Day}/{dateTimeTo.Month}/{dateTimeTo.Year}";

                orderStatistics = _context.Orders
                                        .Include(o => o.OrderProducts)
                                        .ThenInclude(op => op.Product)
                                        .Where(o => o.TimeOrder > dateTimeFrom && o.TimeOrder < dateTimeTo)
                                        .ToList();
            }
            return View(orderStatistics);
        }


    }
}

