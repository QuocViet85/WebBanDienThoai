using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Areas.Product.Model;
using App.Models;
using App.Models.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppMvc.Net.Areas.Product.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewProductController> _logger;
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly CartService _cartService;

        private readonly IEmailSender _sendMailService;

        [TempData]
        public string? StatusMessage {set; get;}

        public ViewProductController(ILogger<ViewProductController> logger, AppDbContext context, CartService cartService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailSender sendMailService)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
            _userManager = userManager;
            _signInManager = signInManager;
            _sendMailService = sendMailService;
        }

        private List<CategoryProduct> GetCategoryProducts()
        {
            var categoryProducts = _context.CategoryProducts
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            return categoryProducts;
        }

        [Route("/product/{categoryslug?}")]
        public IActionResult Index(string categoryslug, [FromQuery(Name = "p")]int currentPage, int pagesize, string search)
        {
            var categories = GetCategoryProducts();
            ViewBag.categories = categories;
            ViewBag.categoryslug = categoryslug;
            ViewBag.FindProducts = false;

            CategoryProduct category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = _context.CategoryProducts
                        .Where(c => c.Slug == categoryslug)
                        .Include(c => c.CategoryChildren)
                        .FirstOrDefault(); //lấy ra category đang chọn

                if (category == null)
                {
                    return NotFound("Không thấy category");
                }
            }

            var products = _context.Products
                                .Include(p => p.Author)
                                .Include(p => p.Photos)
                                .Include(p => p.ProductCategoryProducts)
                                .ThenInclude(pc => pc.CategoryProduct)
                                .AsQueryable()
                                .OrderByDescending(p => p.DateCreated)
                                .ToList();

            if (category != null)
            {
                var ids = new List<int>();
                category.ChildCategoryIDs(ids, null);
                ids.Add(category.Id);

                products = products.Where(p => p.ProductCategoryProducts.Any(pc => ids.Contains(pc.CategoryProductId))).ToList(); //lấy ra product thuộc category đang chọn và thuộc con cháu của category đang chọn
            }

            int totalProducts = products.Count();
            if (pagesize <= 0)
            {
                pagesize = 6;
            }
            int countPages = (int) Math.Ceiling((double) totalProducts / pagesize);

            if (currentPage > countPages)
            {
                currentPage = countPages;
            }
            if (currentPage < 1)
            {
                currentPage = 1;
            }

            List<ProductModel> productsInPage = null;
            productsInPage = products.Skip((currentPage - 1) * pagesize)
                        .Take(pagesize)
                        .ToList();
            
            if (!string.IsNullOrEmpty(search))
            {
                var productSearch = products
                                .Where(p => p.Title.Contains(search) || p.Description.Contains(search));
                
                totalProducts = productSearch.Count();
                countPages = (int) Math.Ceiling((double) totalProducts / pagesize);

                productsInPage = productSearch.Skip((currentPage - 1) * pagesize)
                                            .Take(pagesize)
                                            .ToList();
                ViewBag.FindProducts = true;
                ViewBag.Search = search;
            }

            var pagingModel = new PagingModel()
            {
                 countpages = countPages,
                 currentpage = currentPage,
                 generateUrl = (pageNumber) => Url.Action("Index", new {
                    p = pageNumber,
                    pagesize = pagesize
                 })
            };
            
            ViewBag.pagingModel = pagingModel;

            ViewBag.category = category;
            
            return View(productsInPage.ToList());
        }

        [Route("/product/{productslug}.html")]
        public IActionResult Detail(string productslug)
        {
            var categories = GetCategoryProducts();
            ViewBag.categories = categories;

            var product = _context.Products
                        .Where(p => p.Slug == productslug)
                        .Include(p => p.Author)
                        .Include(p => p.Photos)
                        .Include(p => p.ProductAttributeValues)
                        .ThenInclude(pav => pav.ProductAttribute)
                        .Include(p => p.ProductCategoryProducts)
                        .ThenInclude(pc => pc.CategoryProduct)
                        .FirstOrDefault();
            
            if (product == null)
            {
                return NotFound("Không thấy sản phẩm");
            }

            product.SortProductAttribute();

            CategoryProduct category = product.ProductCategoryProducts.FirstOrDefault()?.CategoryProduct;
            ViewBag.category = category;

            var otherProducts = _context.Products.Where(p => p.ProductCategoryProducts.Any(pc => pc.CategoryProduct.Id == category.Id))
            .Where(p => p.ProductID != product.ProductID)
            .OrderByDescending(p => p.DateUpdated)
            .Take(5); //lấy ra các sản phẩm có cùng chuyên mục với sản phẩm đang chọn

            ViewBag.otherProducts = otherProducts;

            return View(product);
        }

        //Action đặt hàng
        [Route("/addcart/{productid:int}", Name = "AddCart")]
        public IActionResult AddToCart(int productid)
        {
            var product = _context.Products
                        .Where(p => p.ProductID == productid)
                        .FirstOrDefault();

            if (product == null)
            {
                return NotFound("Không thấy sản phẩm");
            }

            var cart = _cartService.GetCartItems();
            var cartItem = cart.Find(c => c.product.ProductID == productid);
            if (cartItem != null)
            {
                cartItem.quantity++;
            }
            else 
            {
                cart.Add(new CartItem
                {
                    quantity = 1,
                    product = product
                });
            }
            _cartService.SaveCartSession(cart);

            return RedirectToAction(nameof(Cart));
        }

        //Trang hiển thị giỏ hàng
        [Route("/cart", Name = "cart")]
        public async Task<IActionResult> Cart()
        {
            RedirectToAction("RemoveExpiredVoucher", "Voucher", new {area = "Product"});
            AppUser user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
            List<Voucher> vouchersOfUser = _context.Vouchers
                                        .Where(v => v.UserId == user.Id && v.OrderId == null && v.PercentageDiscount != 0)
                                        .ToList();
            
            ViewBag.vouchersOfUser = vouchersOfUser;
            }
            
            ViewBag.User = user;

            return View(_cartService.GetCartItems());
        }

        //Xóa sản phẩm
        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart(int productid)
        {
            var carts = _cartService.GetCartItems();
            var cartItem = carts.Find(c => c.product.ProductID == productid);
            if (cartItem != null)
            {
                carts.Remove(cartItem);
            }
            _cartService.SaveCartSession(carts);

            return RedirectToAction(nameof(Cart));
        }

        //Cập nhật giỏ hàng
        [HttpPost]
        [Route("/updatecart", Name = "updatecart")]
        public IActionResult UpdateCart(int productid, int quantity)
        {
            var carts = _cartService.GetCartItems();
            var cartItem = carts.Find(c => c.product.ProductID == productid);
            if (cartItem != null)
            {
                if (cartItem.product.Quantity >= quantity)
                {
                    cartItem.quantity = quantity;
                }
                else 
                {
                    return Json(new {
                        success = 1,
                        enough = false,
                        message = $"Hiện shop chỉ còn {cartItem.product.Quantity} sản phẩm {cartItem.product.Title}. Mời bạn chọn lại số lượng sản phẩm!"
                    });
                }
            }
            _cartService.SaveCartSession(carts);
            return Json(new {
                success = 1,
                enough = true
            });
        }


        //[Bind("VoucherIds")] CreateOrder createOrder
        //Xác nhận đơn hàng
        [Route("/cartconfirm")]
        [HttpPost]
        public async Task<IActionResult> CartConfirm(string? UserId, int[] VoucherIds)
        {
            List<CartItem> listCarts = _cartService.GetCartItems();
            ViewBag.ListCartItems = listCarts;
            if (listCarts.Count == 0)
            {
                return NotFound("Không có mặt hàng nào được chọn");
            }

            if (_signInManager.IsSignedIn(User))
            {
                AppUser user = await _userManager.GetUserAsync(User);
                if (user.Id != UserId || string.IsNullOrEmpty(UserId))
                {
                    return NotFound();
                }
                ViewBag.user = user;
            }

            if (VoucherIds != null)
            {
                //Chuyển mảng các VoucherId thành chuỗi với mỗi phần từ các nhau bằng khoảng trắng
                int totalDiscount = 0;
                string voucherIdsString = " ";
                foreach (int voucherId in VoucherIds)
                {
                    totalDiscount += _context.Vouchers
                                    .Where(v => v.VoucherId == voucherId)
                                    .Select(v => v.PercentageDiscount)
                                    .FirstOrDefault();
                    voucherIdsString += voucherId + " ";
                }
                ViewBag.PercentageDiscount = totalDiscount;
                ViewBag.VoucherIdsString = voucherIdsString.Trim(); //chuỗi các VoucherId của User
            }

            return View();
        }

        [Route("/orderconfirm")]
        [HttpPost]
        public async Task<IActionResult> OrderConfirm([Bind("CustomeName,Address,PhoneNumber,Email,UserId,Total")] Order order, string? voucherIdsString)
        {
            List<CartItem> listCarts = _cartService.GetCartItems();
            if (listCarts.Count == 0)
            {
                return NotFound("Không có mặt hàng nào được chọn");
            }
            ViewBag.ListCartItems = listCarts;

            if (_signInManager.IsSignedIn(User))
            {
                AppUser userCurrent = await _userManager.GetUserAsync(User);
                if (userCurrent.Id != order.UserId)
                {
                    return NotFound();
                }
            }

            foreach (var modelStateKey in ModelState.Keys)
            {
                var modelStateVal = ModelState[modelStateKey];
                foreach (var error in modelStateVal.Errors)
                {
                    var key = modelStateKey;
                    var errorMessage = error.ErrorMessage;
                    Console.WriteLine(key + " - " + errorMessage);
                }
            }
            
            int totalDiscount = 0;
            if (ModelState.IsValid)
            {
                order.TimeOrder = DateTime.Now;
                _context.Orders.Add(order);

                _cartService.ClearCart();

                _context.SaveChanges();

                foreach (var cartItem in listCarts)
                {
                    _context.OrderProducts.Add(new OrderProduct()
                    {
                        ProductId = cartItem.product.ProductID,
                        Quantity = cartItem.quantity,
                        OrderId = order.OrderId,
                    });
                }
                _context.SaveChanges();

                
                //Lấy lại các VoucherId có kiểu int User đã sử dụng cho đơn đặt hàng. Sau đó thiết lập OrderId cho các voucher này 
                if (!string.IsNullOrEmpty(voucherIdsString))
                {
                    string[] arrayVoucherIds = voucherIdsString.Split(" ");
                    foreach (var voucherIdString in arrayVoucherIds)
                    {
                        int voucherId = int.Parse(voucherIdString);
                        var voucher = _context.Vouchers
                                        .Where(v => v.VoucherId == voucherId)
                                        .FirstOrDefault();
                        if (voucher != null)
                        {
                            voucher.OrderId = order.OrderId;
                        }
                        totalDiscount += voucher.PercentageDiscount;
                    }
                    _context.SaveChanges(); 
                    //Phải SaveChanges() 3 lần nếu không sẽ lỗi vì OrderId trong bảng Voucher và bảng OrderProduct là khóa ngoại liên kết đến bảng Order. Vì vậy cần tạo trường dữ liệu Order trước rồi mới có thể thiết lập OrderId của Order đó cho trường dữ liệu Voucher và OrderProduct
                }

                StatusMessage = "Đặt hàng thành công";

                // if (orders.Email != null)
                // {
                //     await _sendMailService.SendEmailAsync(orders.Email, "Thông tin đơn hàng", orders.ContentCart);
                //     StatusMessage += ". Thông tin đơn hàng đã được gửi đến Email của bạn";
                // }
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, "Kiểm tra lại thông tin đã nhập");
            ViewBag.PercentageDiscount = totalDiscount;
            return View("CartConfirm", order);
        }

        [Route("/Order/{UserId?}")]
        public async Task<IActionResult> OrderInformation(string UserId)
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return Content("Bạn cần có tài khoản để sử dụng chức năng này");
            }
            
            AppUser user = _context.Users
                            .Where(u => u.Id == UserId)
                            .Include(u => u.Orders)
                            .ThenInclude(o => o.OrderProducts)
                            .ThenInclude(op => op.Product)
                            .Include(o => o.Vouchers)
                            .FirstOrDefault();
            
            ViewBag.UserName = await _userManager.GetUserNameAsync(user);

            if (user == null)
            {
                return NotFound();
            }

            List<Order> listOrders = user.Orders;

            return View(listOrders);
        }

        private void SendOrderToEmail()
        {

        }
    }
}