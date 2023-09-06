using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using App.Models;
using App.Models.Product;
using Microsoft.EntityFrameworkCore;

namespace App.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger; //dịch vụ logger luôn được đăng kí sẵn vào DI của ứng dụng vì vậy khi cần sử dụng dịch vụ thì chỉ cần inject vào
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index(string search)
    {
        List<ProductModel> listProducts = null;
        listProducts = _context.Products
                        .OrderByDescending(p => p.DateCreated)
                        .Take(8)
                        .Include(p => p.Photos)
                        .Include(p => p.ProductCategoryProducts)
                        .ThenInclude(pc => pc.CategoryProduct)
                        .ToList();

        if (listProducts == null)
        {
            return View();
        }

        ViewBag.FindProducts = false;

        if (!string.IsNullOrEmpty(search))
        {
            listProducts = _context.Products
                            .OrderByDescending(p => p.DateCreated)
                            .Where(p => p.Title.Contains(search) || p.Description.Contains(search))
                            .Include(p => p.Photos)
                            .Include(p => p.ProductCategoryProducts)
                            .ThenInclude(pc => pc.CategoryProduct)
                            .ToList();
            ViewBag.FindProducts = true;
        }

        return View(listProducts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public string HiHome()
    {
        return "Xin chao cac ban, toi HiHome";
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
