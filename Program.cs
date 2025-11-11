
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext;
using PcSaler.Interfaces;
using PcSaler.Repository;
using PcSaler.Services;
using PcSaler.Management_System.Interfaces;
using PcSaler.Management_System.Repository;
using PcSaler.Management_System.Services;

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

            // Register service and repository
            builder.Services.AddScoped<ICategoryService, Repository_Category>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<IProductService, Repository_Product>();
            builder.Services.AddScoped<ProductService>();

            // Register Management System Repositories
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderStatusRepository, OrderStatusRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPCBuildRepository, PCBuildRepository>();

            // Register Management System Services
            builder.Services.AddScoped<CategoryManagementService>();
            builder.Services.AddScoped<ProductManagementService>();
            builder.Services.AddScoped<CustomerService>();
            builder.Services.AddScoped<OrderService>();
            builder.Services.AddScoped<OrderStatusService>();
            builder.Services.AddScoped<PaymentService>();
            builder.Services.AddScoped<PCBuildManagementService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();

            // Route cho Management Area
            app.MapControllerRoute(
                name: "management",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
