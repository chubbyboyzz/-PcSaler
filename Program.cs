using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Repository;
using PcSaler.Services;

namespace PcSaler
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =========================================================
            // 1. CẤU HÌNH DATABASE
            // =========================================================
            builder.Services.AddDbContext<PCShopContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("default")));

            // =========================================================
            // 2. CẤU HÌNH XÁC THỰC (AUTHENTICATION) - GỘP CHUNG TẠI ĐÂY
            // =========================================================

            // Lấy cấu hình Google từ appsettings.json
            var googleConfig = builder.Configuration.GetSection("GoogleKeys");

            builder.Services.AddAuthentication(options =>
            {
                // Thiết lập mặc định là dùng Cookie
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options => // Cấu hình Cookie
            {
                options.LoginPath = "/Login/Index";      // Chưa đăng nhập thì về đây
                options.AccessDeniedPath = "/Home/AccessDenied"; // Không có quyền thì về đây
                options.ExpireTimeSpan = TimeSpan.FromDays(3);
                options.Cookie.HttpOnly = true;
            })
            .AddGoogle(options => // Cấu hình Google (Nối đuôi ngay sau Cookie)
            {
                options.ClientId = googleConfig["ClientId"];
                options.ClientSecret = googleConfig["ClientSecret"];
                //link đã đăng kí trên cloud google
                options.CallbackPath = "/signin-google"; 
            });

            // =========================================================

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Cấu hình Session (Giữ nguyên của ông)
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Register service and repository (Giữ nguyên)
            builder.Services.AddScoped<ICategoryService, Repository_Category>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<IProductService, Repository_Product>();
            builder.Services.AddScoped<ProductService>();

            // Service Hash & Login
            builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>(); // Nhớ đảm bảo class này đã có constructor nhận IConfiguration nếu dùng Pepper
            builder.Services.AddScoped<ILoginService, Repository_Login>();
            builder.Services.AddScoped<LoginService>();

            builder.Services.AddScoped<ICartService, Repository_Cart>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IOrderService, OrderService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            // Thứ tự quan trọng: Session -> AuthN -> AuthZ
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}