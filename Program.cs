
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.DBcontext.Entites;
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



            //add cookie
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
            // Register service and repository
            builder.Services.AddScoped<ICategoryService, Repository_Category>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<IProductService, Repository_Product>();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<ILoginService, Repository_Login>();
            builder.Services.AddScoped<LoginService>();
            builder.Services.AddScoped<ICartService, Repository_Cart>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddScoped<IPcBuildService, Repository_PcBuild>();
            builder.Services.AddScoped<PcBuildService>();
            builder.Services.AddScoped<ICustomerPCService, Repository_CustomersPC>();
            builder.Services.AddScoped<CustomerPCService>();
            builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>(); // Nhớ đảm bảo class này đã có constructor nhận IConfiguration nếu dùng Pepper
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            // Email sender service
            builder.Services.AddTransient<IEmailSender, EmailSender>();
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
