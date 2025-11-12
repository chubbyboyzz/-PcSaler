// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Khai báo hàm để gom các logic liên quan đến trang chủ
const HomePageManager = (() => {

    // Khởi tạo Carousel (Banner)
    const initCarousel = () => {
        // Kiểm tra xem có Carousel ID này không
        const mainCarousel = document.getElementById('mainCarousel');
        if (mainCarousel) {
            // Khởi tạo Bootstrap Carousel và thiết lập tự động trượt sau 5 giây (5000ms)
            new bootstrap.Carousel(mainCarousel, {
                interval: 5000,
                pause: 'hover' // Dừng khi di chuột vào
            });
            console.log("Main Carousel initialized successfully.");
        }
    };

    // Xử lý các lỗi cơ bản về giao diện
    const initUIFixes = () => {
        // Tối ưu hóa ảnh bị lỗi (Fallback cho ảnh không load được)
        document.querySelectorAll('img').forEach(img => {
            img.onerror = function () {
                // Thay thế ảnh bị lỗi bằng ảnh "no-image" mặc định
                this.src = '/images/no-image.png';
                this.onerror = null; // Ngăn ngừa vòng lặp vô tận nếu ảnh fallback cũng lỗi
            };
        });

        // Xử lý nút "Xem thêm" trên trang chủ để đảm bảo link đúng
        document.querySelectorAll('.btn-outline-secondary').forEach(btn => {
            btn.addEventListener('click', (e) => {
                // Có thể thêm tracking hoặc animation tại đây nếu cần
                console.log("Navigating to category products...");
            });
        });
    };

    // Hàm khởi chạy tất cả
    const initialize = () => {
        initCarousel();
        initUIFixes();
        // Thêm các hàm xử lý khác ở đây
    };

    // Trả về hàm initialize để gọi từ bên ngoài
    return {
        init: initialize
    };

})(); // Immediate Invoked Function Expression (IIFE)

// Chờ cho toàn bộ DOM được tải xong
document.addEventListener('DOMContentLoaded', () => {
    // Chỉ khởi chạy các chức năng của trang chủ khi cần
    HomePageManager.init();
    console.log("Site scripts initialized.");
});