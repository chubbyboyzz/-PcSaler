
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

            // Add Database
            builder.Services.AddDbContext<PCShopContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("default")));

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //add cookie
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/Login/Index"; // Chuyển hướng về đây nếu chưa đăng nhập
                options.AccessDeniedPath = "/Home/AccessDenied"; // Chuyển hướng nếu không có quyền
                options.ExpireTimeSpan = TimeSpan.FromDays(7); // Cookie tồn tại 7 ngày
                options.Cookie.HttpOnly = true;
            });
            builder.Services.AddDistributedMemoryCache(); // Bắt buộc phải có Cache để dùng Session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session tồn tại 30 phút
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            // Register service and repository
            builder.Services.AddScoped<ICategoryService, Repository_Category>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<IProductService, Repository_Product>();
            builder.Services.AddScoped<ProductService>();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();


            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
