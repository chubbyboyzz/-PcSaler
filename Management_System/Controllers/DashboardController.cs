using Microsoft.AspNetCore.Mvc;
using PcSaler.Management_System.Services;

namespace PcSaler.Management_System.Controllers
{
    [Area("Management")]
    public class DashboardController : Controller
    {
        private readonly CategoryManagementService _categoryService;
        private readonly ProductManagementService _productService;
        private readonly CustomerService _customerService;
        private readonly OrderService _orderService;
        private readonly PaymentService _paymentService;
        private readonly PCBuildManagementService _pcBuildService;

        public DashboardController(
            CategoryManagementService categoryService,
            ProductManagementService productService,
            CustomerService customerService,
            OrderService orderService,
            PaymentService paymentService,
            PCBuildManagementService pcBuildService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _customerService = customerService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pcBuildService = pcBuildService;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy thống kê tổng quan
            ViewBag.TotalCategories = await _categoryService.GetTotalCategoriesAsync();
            ViewBag.TotalProducts = await _productService.GetTotalProductsAsync();
            ViewBag.TotalCustomers = await _customerService.GetTotalCustomersAsync();
            ViewBag.TotalOrders = await _orderService.GetTotalOrdersAsync();
            ViewBag.TotalRevenue = await _paymentService.GetTotalRevenueAsync();
            ViewBag.TotalPCBuilds = await _pcBuildService.GetTotalPCBuildsAsync();

            // Lấy đơn hàng gần đây
            var allOrders = await _orderService.GetAllOrdersAsync();
            ViewBag.RecentOrders = allOrders.Take(10).ToList();

            // Dữ liệu cho biểu đồ doanh thu 6 tháng gần đây
            var monthlyRevenue = await GetMonthlyRevenueAsync(6);
            ViewBag.MonthlyRevenueLabels = monthlyRevenue.Select(x => x.Month).ToList();
            ViewBag.MonthlyRevenueData = monthlyRevenue.Select(x => x.Revenue).ToList();

            // Dữ liệu cho biểu đồ sản phẩm theo danh mục
            var productsByCategory = await GetProductsByCategoryAsync();
            ViewBag.CategoryLabels = productsByCategory.Select(x => x.CategoryName).ToList();
            ViewBag.CategoryData = productsByCategory.Select(x => x.ProductCount).ToList();

            // Dữ liệu cho biểu đồ trạng thái đơn hàng
            var ordersByStatus = await GetOrdersByStatusAsync();
            ViewBag.OrderStatusLabels = ordersByStatus.Select(x => x.StatusName).ToList();
            ViewBag.OrderStatusData = ordersByStatus.Select(x => x.Count).ToList();

            return View();
        }

        private async Task<List<MonthlyRevenue>> GetMonthlyRevenueAsync(int months)
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            var result = new List<MonthlyRevenue>();
            
            for (int i = months - 1; i >= 0; i--)
            {
                var targetDate = DateTime.Now.AddMonths(-i);
                var monthRevenue = payments
                    .Where(p => p.PaymentDate.Year == targetDate.Year && p.PaymentDate.Month == targetDate.Month)
                    .Sum(p => p.Amount);
                
                result.Add(new MonthlyRevenue
                {
                    Month = targetDate.ToString("MM/yyyy"),
                    Revenue = monthRevenue
                });
            }
            
            return result;
        }

        private async Task<List<CategoryProductCount>> GetProductsByCategoryAsync()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var products = await _productService.GetAllProductsAsync();
            
            var result = categories.Select(c => new CategoryProductCount
            {
                CategoryName = c.CategoryName,
                ProductCount = products.Count(p => p.CategoryID == c.CategoryID)
            }).Where(x => x.ProductCount > 0).ToList();
            
            return result;
        }

        private async Task<List<OrderStatusCount>> GetOrdersByStatusAsync()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            
            var result = orders
                .GroupBy(o => o.CurrentStatus?.StatusName ?? "Chưa xác định")
                .Select(g => new OrderStatusCount
                {
                    StatusName = g.Key,
                    Count = g.Count()
                })
                .ToList();
            
            return result;
        }

        // Helper classes cho dữ liệu biểu đồ
        private class MonthlyRevenue
        {
            public string Month { get; set; }
            public decimal Revenue { get; set; }
        }

        private class CategoryProductCount
        {
            public string CategoryName { get; set; }
            public int ProductCount { get; set; }
        }

        private class OrderStatusCount
        {
            public string StatusName { get; set; }
            public int Count { get; set; }
        }
    }
}
