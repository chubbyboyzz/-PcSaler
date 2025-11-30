using System.ComponentModel.DataAnnotations;

namespace PcSaler.Models
{
    public class UpdateProfileViewModel
    {
        [Required(ErrorMessage = "Name can't not be empty")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Numbers can't not be empty")]
        [RegularExpression(@"(84|0[3|5|7|8|9])+([0-9]{8})\b", ErrorMessage = "Number wrong type")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Adress can't not be empty")]
        [MinLength(10, ErrorMessage = "Adress must be detailed (more than 10 letters)")]
        public string Address { get; set; }

        // Email thường không cho sửa vì liên quan đến tài khoản đăng nhập
    }
}