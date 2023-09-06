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
    public class VoucherController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly AppDbContext _context;
        public VoucherController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }
        
        [TempData]
        public string StatusMessage {set; get;}

        [Route("/product/luck")] 
        public async Task<IActionResult> Index()
        {
            RemoveExpiredVoucher();
            ViewBag.SignedIn = _signInManager.IsSignedIn(User);
            bool CanDraw = true;
            List<Voucher> listVouchers = null;
            if (_signInManager.IsSignedIn(User))
            {
                AppUser user = await _userManager.GetUserAsync(User);
                DateTime timenow = DateTime.Now;
                listVouchers = _context.Vouchers
                                        .Where(v => v.UserId == user.Id)
                                        .ToList();

                foreach (var voucher in listVouchers)
                {
                    DateTime newDrawDate = voucher.LastDrawDate.AddDays(1);
                    int compare = DateTime.Compare(timenow, newDrawDate);

                    if (compare < 0)
                    {
                        CanDraw = false;
                    }
                }
            }
            ViewBag.CanDraw = CanDraw;
            return View(listVouchers);
        }

        [Route("/product/random")]
        [HttpPost]
        public async Task<IActionResult> Random()
        {
            AppUser user = await _userManager.GetUserAsync(User);
            
            if (user == null)
            {
                return NotFound();
            }

            int[] giamGiaArray = new int[13] {0, 5, 0, 10, 0, 15, 0, 20, 0, 25, 0, 30, 0};
            Random randomLuck = new Random();
            int giamGia = giamGiaArray[randomLuck.Next(0, 12)];

            DateTime timenow = DateTime.Now;

            if (giamGia != 0)
            {
                Voucher voucher = new Voucher()
                {
                    PercentageDiscount = giamGia,
                    UserId = user.Id,
                    LastDrawDate = timenow,
                    ExpiredDate = timenow.AddDays(3)
                };
                AddVoucherToDatabase(voucher);
                StatusMessage = $"Bạn đã trúng thưởng voucher giảm giá trị giá: {giamGia} %";
            }
            else 
            {
                Voucher voucher = new Voucher()
                {
                    UserId = user.Id,
                    LastDrawDate = timenow,
                    ExpiredDate = timenow.AddDays(1)
                };
                AddVoucherToDatabase(voucher);
                StatusMessage = "Chúc bạn may mắn lần sau!";
            }
            
            return RedirectToAction(nameof(Index));
        }

        public void AddVoucherToDatabase(Voucher voucher)
        {
            _context.Vouchers.Add(voucher);
            _context.SaveChanges();
        }


        [Route("/product/removexpiredvoucher")]
        public void RemoveExpiredVoucher()
        {
            try
            {
                DateTime timenow = DateTime.Now;
                List<Voucher> vouchers = _context.Vouchers.ToList();
                List<Voucher> vouchersRemove = new List<Voucher>();
                foreach (var voucher in vouchers)
                {
                    int compare = DateTime.Compare(timenow, voucher.ExpiredDate);
                    if (compare >= 0 && voucher.OrderId == null)
                    {
                        vouchersRemove.Add(voucher);
                    }
                }
                _context.Vouchers.RemoveRange(vouchersRemove);
                _context.SaveChanges();
            }
            catch (Exception er)
            {

            }
        }
    }
}