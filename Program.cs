using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using AppMvc.Net.Areas.Product.Controllers;

internal class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews(); //đăng kí các dịch vụ để hoạt động theo mô hình MVC
        builder.Services.AddRazorPages(); //đăng kí dịch vụ sử dụng RazorPage

        var connectionString = builder.Configuration.GetConnectionString("AppMvcConnectionString");

        builder.Services.AddDbContext<AppDbContext>(options => {
            options.UseSqlServer(connectionString);
            options.EnableSensitiveDataLogging();
        });

        builder.Services.Configure<RazorViewEngineOptions>(options => 
        {
            //Thiết lập đường dẫn để View() trong Controller tìm file razor (.cshtml) nếu như tìm trong đường dẫn mặc định không có file razor cần tìm

            //{0} - tên Action
            //{1} - tên Controller
            //{2} - tên Area
            options.ViewLocationFormats.Add("/MyView/{1}/{0}.cshtml");
        }); 

        //Đăng kí Identity
        builder.Services.AddIdentity<AppUser, IdentityRole>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();

        //Thiết lập 1 số cấu hình cho các dịch vụ Identity
        // Truy cập IdentityOptions
        builder.Services.Configure<IdentityOptions>(options =>
        {
            // Thiết lập về Password
            options.Password.RequireDigit = false; // Không bắt phải có số
            options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
            options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
            options.Password.RequireUppercase = false; // Không bắt buộc chữ in
            options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
            options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

            // Cấu hình Lockout - khóa user
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
            options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 3 lầ thì khóa
            options.Lockout.AllowedForNewUsers = true;

            // Cấu hình về User.
            options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = true;  // Email là duy nhất

            // Cấu hình đăng nhập.
            options.SignIn.RequireConfirmedEmail = false;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
            options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại
            options.SignIn.RequireConfirmedAccount = false;         // Xác thực tài khoản
        });

        // Để sử dụng Attribute thiết lập quyền truy cập [Authorize] thì cần cấu hình như sau
        builder.Services.ConfigureApplicationCookie(options => {
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/khongduoctruycap.html";
        });

        //Đăng kí dịch vụ Email
        builder.Services.AddOptions();
        var mailSettings = builder.Configuration.GetSection("MailSettings");
        builder.Services.Configure<MailSettings>(mailSettings);
        builder.Services.AddSingleton<IEmailSender, SendMailService>();

        //Đăng kí dịch vụ xác thực từ Google vào ứng dụng
        builder.Services.AddAuthentication();

        //Đăng kí dịch vụ thông báo lỗi Identity
        builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

        builder.Services.AddAuthorization(options => {
            options.AddPolicy("Admin", policyBuilder => {
                policyBuilder.RequireAuthenticatedUser();
                policyBuilder.RequireRole(RoleName.Administrator);
            });

            options.AddPolicy("ViewManageMenu", policyBuilder => {
                policyBuilder.RequireAuthenticatedUser();
                policyBuilder.RequireRole(new string[]{RoleName.Administrator, RoleName.Moderator});
            });
        });

        //Đăng kí dịch vụ ProductService. Có thể lấy ra dịch vụ này (tự động khởi tạo đối tượng dịch vụ) và inject dịch vụ
        // builder.Services.AddSingleton<ProductService, ProductService>();

        builder.Services.AddDistributedMemoryCache(); // Đăng kí dịch vụ lưu cache trong bộ nhớ
        builder.Services.AddSession(cfs => {          // Đăng kí dịch vụ Session
            cfs.Cookie.Name = "appmvc";               //Đặt tên Session - tên này được sử dụng ở Client (Cookie Session)
            cfs.IdleTimeout = new TimeSpan(0, 30, 0); //Thời gian tồn tại của Session
        });

        builder.Services.AddTransient<CartService>(); //Dăng kí dịch vụ CartService để xây dựng chức năng giỏ hàng, đặt hàng

        builder.Services.AddTransient<IActionContextAccessor, ActionContextAccessor>(); //đăng kí dịch vụ IActionContextAccessor để tạo ra dịch vụ AdminSidebarService
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles(); // Files in wwwroot (~)

        app.UseStaticFiles(new StaticFileOptions()
        {
            FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
            ),
            RequestPath = "/contents" //Thiết lập đoạn Url chung cho các file
        }); // Files in Uploads

        app.UseSession();
        
        app.AddStatusCodePage(); //tùy biến Response khi có lỗi: 400 - 599

        app.UseRouting(); //EndpointRoutingMiddleware

        app.UseAuthentication(); //xác định danh tính
        app.UseAuthorization(); //xác thực quyền truy cập

        // Middleware trực tiếp đưa Request đến các Controller, Action trong ứng dụng
        app.MapControllerRoute(
            name : "default",
            pattern : "/{controller=Home}/{action=Index}/{id?}" //thiết lập này chỉ có hiệu lực với các Controller có trong thư mục Controller
        ); 

        app.MapRazorPages(); //endpoint (Middleware cuối) tới trang Razor trên ứng dụng

        app.Run();
    }
}


/*
* Lấy đường dẫn đến thư mục đang chạy ứng dụng Web này: 
    string ContentRootPath = (IWebHostEnvironment) _env.ContentRootPath;

* Xây dựng các Endpoint để ánh xạ URL vào controller:
Có các phương thức:
- app.MapControllers()
- app.MapControllerRoute()
- app.MapDefaultControllerRoute()
- app.MapAreaControllerRoute()
)

* Một số Attribute sử dụng trong Controller hoặc Action:
[AcceptVerb]
[Route]
[HttpGet]
[HttpPost]
[HttpPut]
[HttpDelete]
[HttpHead]
[HttpPatch]  

** [AcceptVerb] thiết lập phương thức truy cập hợp lệ với action trong controller 

** [Route]:
- [Route] thiết lập địa chỉ truy cập URL cho các controller và action trong controller. Khi thiết lập Attribute [Route] này cho action thì sự ảnh hưởng của Route khai báo trong MapController đến action không còn
- [Route] khi dùng với cả controller và action của controller, nếu Route của action không khai báo url tuyệt đối (bằng cách có ký tự / trước Url) thì Url cùa action = Urlcontroller/Urlaction
- Khi đã thiết lập [Route] ở controller hoặc không thiết lập [Route] ở Controller thì [Route] ở action phải thiết lập nếu không sẽ lỗi. Nếu thiết lập [Route] ở controller và không thiết lập [Route] ở action mà vẫn muốn truy cập được action thì [Route] ở controller phải cho vào token [action] thì có thể truy cập action bằng Url = Urlcontroller/Tên action
- [Route] có thể khai báo nhiều lần cho 1 action
- Các tham số đặc biệt của [Route]: [controller] [action] [area] => các tham số này đại diện cho tên thật của vấn đề tương ứng
- Bản chất của [Route] là tạo ra các Endpoint để ánh xạ URL vào Controller và Action
- Tham số được chứa trong {}

** [HttpGet], [HttpPost], [HttpPut], [HttpDelete], [HttpHead], [HttpPatch]
- Thiết lập phương thức truy vấn và địa chỉ truy vấn hợp lệ

** [AllowAnonymous]: thiết lập controller, action có thể được truy cập bởi bất kỳ ai, không bị cần phải xác thực gì cả

** [Area]

* Phát sinh Url từ Controller/Action với UrlHeplper: Url 
* Phát sinh Url từ Controller/Action với tên Route
* Địa chỉ URl phát sinh từ tagHeler, có những thuộc tính sau:
- asp-area="Area"
- asp-action="Action"
- asp-controller="Controller"
- asp-route... (id, name,...)
- asp-route="default //Tên route



* Lệnh phát sinh controller: dotnet aspnet-codegenerator controller -name Planet -namespace App.Controllers -outDir Controller 

* Lệnh phát sinh controller có chức năng CRUD: dotnet aspnet-codegenerator controller -name Contact -namespace App.Areas.Contact.Controllers -m App.Models.Contacts.ContactModel -udl -dc App.Models.AppDbContext -outDir Areas/Contact/Controllers

"libraries": [
    {
      "library": "multiple-select@1.2.3",
      "destination": "wwwroot/lib/multiple-select"
    },
    {
      "library": "summernote@0.8.20",
      "destination": "wwwroot/lib/summernote"
    },
    {
      "library": "jqueryui@1.13.2",
      "destination": "wwwroot/lib/jqueryui"
    },
    {
      "library": "elfinder@2.1.61",
      "destination": "wwwroot/lib/elfinder"
    }
    ,
    {
      "library": "jquery-easing@1.4.1",
      "destination": "wwwroot/lib/jquery-easing"
    }
    ,
    {
      "library": "font-awesome@5.15.3",
      "destination": "wwwroot/lib/font-awesome"
    }
  ]
*/