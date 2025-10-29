using Microsoft.EntityFrameworkCore;
using PcSaler.DBcontext.Entites;
namespace PcSaler.DBcontext
{
    public class PCShopContext : DbContext
    {
        public PCShopContext(DbContextOptions<PCShopContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<PCBuild> PCBuilds { get; set; }
        public DbSet<PCBuildDetail> PCBuildDetails { get; set; }

        public DbSet<CustomPC> CustomPCs { get; set; }
        public DbSet<CustomPCDetail> CustomPCDetails { get; set; }

        public DbSet<Cart> Carts { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bảng tương ứng với tên SQL đã tạo
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Product>().ToTable("Products");

            modelBuilder.Entity<PCBuild>().ToTable("PCBuild");
            modelBuilder.Entity<PCBuildDetail>().ToTable("PCBuildDetails");

            modelBuilder.Entity<CustomPC>().ToTable("CustomPC");
            modelBuilder.Entity<CustomPCDetail>().ToTable("CustomPCDetails");

            modelBuilder.Entity<Cart>().ToTable("Cart");

            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");

            modelBuilder.Entity<OrderStatus>().ToTable("OrderStatus");
            modelBuilder.Entity<OrderStatusHistory>().ToTable("OrderStatusHistory");

            modelBuilder.Entity<Payment>().ToTable("Payments");

            // Thiết lập ràng buộc & cascade rules (nếu cần)
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

            // Nếu Product có navigation collections to PCBuildDetails or CustomPCDetails,
            // bạn có thể cấu hình thêm tương tự.  (Ở ví dụ này Product không khai báo collection để giữ đơn giản)
        }
    }

}
