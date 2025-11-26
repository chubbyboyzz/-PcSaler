using System;
using System.Collections.Generic;

namespace PcSaler.Models
{
    // 1. ViewModel chứa thông tin cá nhân của khách
    public class CustomerProfileViewModel
    {
        public int CustomerID { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // 2. ViewModel cho từng dòng lịch sử đơn hàng
    public class OrderHistoryViewModel
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string StatusName { get; set; } // Tên trạng thái (VD: Đã giao, Đang xử lý)
        public string StatusColorClass { get; set; } // Class CSS để tô màu trạng thái (sẽ xử lý ở Service)
    }

    // 3. ViewModel TỔNG HỢP cho cả trang Profile
    public class ProfilePageViewModel
    {
        public CustomerProfileViewModel CustomerInfo { get; set; }
        public List<OrderHistoryViewModel> OrderHistory { get; set; }

        public ProfilePageViewModel()
        {
            OrderHistory = new List<OrderHistoryViewModel>();
        }
    }
}