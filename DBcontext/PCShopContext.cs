using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext.Entites;

namespace PcSaler.DBcontext
{
    public class PCShopContext : DbContext
    {
        public PCShopContext(DbContextOptions<PCShopContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductAttribute> ProductAttributes { get; set; }
        public DbSet<PCBuild> PCBuilds { get; set; }
        public DbSet<PCBuildDetail> PCBuildDetails { get; set; }

        public DbSet<CustomPC> CustomPCs { get; set; }
        public DbSet<CustomPCDetail> CustomPCDetails { get; set; }
        public DbSet<Carts> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; } 

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        public DbSet<Payment> Payments { get; set; }

        public DbSet<PriceRange> PriceRanges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bảng tương ứng với tên SQL đã tạo
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Categories>().ToTable("Categories");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductAttribute>().ToTable("ProductAttributes");

            modelBuilder.Entity<PCBuild>().ToTable("PCBuild");
            modelBuilder.Entity<PCBuildDetail>().ToTable("PCBuildDetails");

            modelBuilder.Entity<CustomPC>().ToTable("CustomPC");
            modelBuilder.Entity<CustomPCDetail>().ToTable("CustomPCDetails");

            // **THAY ĐỔI 2: Đặt tên bảng Carts và CartItems**
            modelBuilder.Entity<Carts>().ToTable("Carts");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");

            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");

            modelBuilder.Entity<OrderStatus>().ToTable("OrderStatus");
            modelBuilder.Entity<OrderStatusHistory>().ToTable("OrderStatusHistory");

            modelBuilder.Entity<Payment>().ToTable("Payments");

            modelBuilder.Entity<PriceRange>().ToTable("PriceRanges");

            // ----------------------------------------------------
            // Thiết lập RÀNG BUỘC VÀ QUAN HỆ (Fluent API)
            // ----------------------------------------------------

            // 1. Cấu hình Unique cho CustomerID trong Carts
            // Mỗi khách hàng chỉ có một giỏ hàng duy nhất
            modelBuilder.Entity<Carts>()
                .HasIndex(c => c.CustomerID)
                .IsUnique();

            // 2. Cấu hình Quan hệ Giỏ hàng - Mục Giỏ hàng (Carts - CartItems)
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartID)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Cấu hình Quan hệ Phân cấp Category (Self-Referencing)
            modelBuilder.Entity<Categories>()
                .HasOne(c => c.ParentCategory)      // Category có một ParentCategory
                .WithMany(c => c.Children)          // ParentCategory có nhiều Children
                .HasForeignKey(c => c.ParentCategoryID)
                .OnDelete(DeleteBehavior.Restrict); // Tránh xóa Category cha khi vẫn còn Category con

            // 4. Cấu hình Quan hệ Orders - CurrentStatus (Đã thêm CurrentStatusID vào Orders)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.CurrentStatus)       // Order có một CurrentStatus
                .WithMany()                         // CurrentStatus có thể thuộc nhiều Order
                .HasForeignKey(o => o.CurrentStatusID)
                .OnDelete(DeleteBehavior.Restrict);

            // 5. Cấu hình các ràng buộc đã có (giữ nguyên)
            modelBuilder.Entity<PCBuildDetail>()
                .HasOne(d => d.PCBuild)
                .WithMany(b => b.Details)
                .HasForeignKey(d => d.PCBuildID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PCBuildDetail>()
                .HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderID)
                .OnDelete(DeleteBehavior.Cascade);

            // Thêm CustomPCDetails (Thường thiết lập như PCBuildDetails)
            modelBuilder.Entity<CustomPCDetail>()
                .HasOne(d => d.CustomPC)
                .WithMany(b => b.Details)
                .HasForeignKey(d => d.CustomPCID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomPCDetail>()
                .HasOne(d => d.Product)
                .WithMany()
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            // Thiết lập Unique cho ComponentType trong Category
            modelBuilder.Entity<Categories>()
                 .HasIndex(c => c.ComponentType)
                 .IsUnique();


            // --- THÊM MỚI (2/2) ---
            // 6. Cấu hình Bảng PriceRanges

            // Thiết lập Unique cho cột Identifier
            modelBuilder.Entity<PriceRange>()
                .HasIndex(pr => pr.Identifier)
                .IsUnique();

            // Thiết lập khóa ngoại CategoryID (cho phép NULL)
            modelBuilder.Entity<PriceRange>()
                .HasOne(pr => pr.Category) // PriceRange có 1 Category (hoặc null)
                .WithMany()                // Một Category có thể có nhiều PriceRanges (hoặc không cần trỏ ngược)
                .HasForeignKey(pr => pr.CategoryID)
                .IsRequired(false) // Quan trọng: Đánh dấu là không bắt buộc (cho phép NULL)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Category nếu đang được PriceRange tham chiếu
            // -----------------------
        }
    }
}