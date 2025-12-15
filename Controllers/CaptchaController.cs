using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace PcSaler.Controllers
{
    public class CaptchaController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public CaptchaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // API 1: Lấy ảnh và cắt mảnh ghép
        [HttpGet]
        [Route("Captcha/GetCaptchaImage")]
        public IActionResult GetCaptchaImage()
        {
            try
            {
                // 1. Lấy ảnh ngẫu nhiên từ thư mục
                string folderPath = Path.Combine(_env.WebRootPath, "images", "captcha");

                // Tự động tạo thư mục nếu chưa có (tránh lỗi crash)
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var files = Directory.GetFiles(folderPath, "*.jpg");
                if (files.Length == 0)
                {
                    // Fallback: Nếu ông quên bỏ ảnh vào thì nó báo lỗi này
                    return BadRequest(new { message = "Chưa có ảnh trong thư mục /images/captcha" });
                }

                // Random 1 ảnh
                string imagePath = files[new Random().Next(files.Length)];

                using (Bitmap original = new Bitmap(imagePath))
                {
                    // Resize ảnh về kích thước cố định cho Web (300x150 px)
                    // Để đảm bảo giao diện lúc nào cũng đẹp, không bị vỡ khung
                    using (Bitmap bmp = new Bitmap(original, new Size(300, 150)))
                    {
                        // 2. Tính toán vị trí cắt mảnh ghép
                        Random rand = new Random();
                        int puzzleWidth = 45;  // Chiều rộng mảnh ghép
                        int puzzleHeight = 45; // Chiều cao mảnh ghép

                        // Random vị trí X (Target) phải nằm bên phải (từ 100px đến 250px)
                        int targetX = rand.Next(100, 250);
                        // Random vị trí Y (Cao độ)
                        int targetY = rand.Next(10, 100);

                        // QUAN TRỌNG: Lưu vị trí đúng vào Session để tí nữa hàm Verify kiểm tra
                        HttpContext.Session.SetInt32("CaptchaTargetX", targetX);

                        // 3. Cắt mảnh ghép (Puzzle Piece)
                        Bitmap piece = new Bitmap(puzzleWidth, puzzleHeight);
                        using (Graphics g = Graphics.FromImage(piece))
                        {
                            // Cắt phần ảnh tại vị trí target
                            g.DrawImage(bmp, new Rectangle(0, 0, puzzleWidth, puzzleHeight),
                                new Rectangle(targetX, targetY, puzzleWidth, puzzleHeight), GraphicsUnit.Pixel);

                            // Vẽ viền cho mảnh ghép nổi bật hơn
                            g.DrawRectangle(Pens.Yellow, 0, 0, puzzleWidth - 1, puzzleHeight - 1);
                        }

                        // 4. Vẽ ô khuyết lên ảnh nền (Background)
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            using (SolidBrush brush = new SolidBrush(Color.FromArgb(180, 0, 0, 0))) // Màu đen bán trong suốt
                            {
                                g.FillRectangle(brush, targetX, targetY, puzzleWidth, puzzleHeight);
                                g.DrawRectangle(Pens.White, targetX, targetY, puzzleWidth, puzzleHeight);
                            }
                        }

                        // 5. Trả về kết quả cho Frontend
                        return Ok(new
                        {
                            bg = BitmapToBase64(bmp),      // Ảnh nền đã bị đục lỗ
                            piece = BitmapToBase64(piece), // Mảnh ghép rời
                            y = targetY                    // Cao độ Y để frontend đặt mảnh ghép đúng dòng
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Lỗi xử lý ảnh: " + ex.Message });
            }
        }

        // API 2: Kiểm tra vị trí thả chuột
        [HttpPost]
        [Route("Captcha/Verify")]
        public IActionResult Verify([FromBody] CaptchaVerifyRequest req)
        {
            // Lấy vị trí đúng từ Session (đã lưu ở bước trên)
            int? correctX = HttpContext.Session.GetInt32("CaptchaTargetX");

            if (correctX == null) return Json(new { success = false, message = "Captcha hết hạn, hãy tải lại." });

            // Cho phép sai số +- 5 pixel (để người dùng dễ thở hơn)
            if (Math.Abs(correctX.Value - req.x) <= 5)
            {
                // Nếu đúng: Tạo "Vé thông hành" (Token)
                string token = Guid.NewGuid().ToString();

                // Lưu vé này vào Session để LoginController kiểm tra
                HttpContext.Session.SetString("CaptchaVerifiedToken", token);

                return Json(new { success = true, token = token });
            }

            // Nếu sai
            return Json(new { success = false, message = "Sai vị trí rồi!" });
        }

        // Hàm phụ: Chuyển ảnh sang chuỗi Base64 để hiển thị trên web
        private string BitmapToBase64(Bitmap bmp)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
            }
        }
    }

    // Class hứng dữ liệu JSON từ client gửi lên
    public class CaptchaVerifyRequest
    {
        public int x { get; set; }
    }
}